using Api.Configuration.Swagger;
using Microsoft.Extensions.Options;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Infrastructure;
using Nakama.Formations.Domain;
using Nakama.Formations.Infrastructure;
using Nakama.LlmProviders.Domain;
using Nakama.LlmProviders.Infrastructure;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure;
using Nakama.Shared.Infrastructure.Chat;
using Nakama.Shared.Infrastructure.Commands;
using Nakama.Shared.Infrastructure.Documents;
using Nakama.Shared.Infrastructure.Events;
using Nakama.Shared.Infrastructure.Icons;
using Nakama.Shared.Infrastructure.Llm;
using Nakama.Shared.Infrastructure.Pagination;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;
using Nakama.Shared.Infrastructure.Queries;
using Nakama.Shared.Infrastructure.Storage;
using Nakama.Shared.Infrastructure.AnswerFeedback;
using Nakama.Shared.Infrastructure.QuestionActivity;
using Nakama.Shared.Infrastructure.Reporting;
using Nakama.Shared.Infrastructure.UserActivity;
using Nakama.Users.Domain;
using Nakama.Users.Infrastructure;

namespace Api.Configuration;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddLogger()
            .AddAuth()
            .AddSwagger()
            .AddEndpoints();

        services.AddHealthChecks()
            .AddNpgSql(
                sp => sp.GetRequiredService<IConfiguration>().GetConnectionString("Nakama")!,
                name: "postgres");

        services.AddExceptionHandler<ApiExceptionHandler>();
        services.AddProblemDetails();

        services.AddOptions<StorageOptions>()
            .BindConfiguration(StorageOptions.SectionName)
            .ValidateOnStart();

        services.AddOptions<LlmOptions>()
            .BindConfiguration(LlmOptions.SectionName)
            .Validate(options => options.MaxTokens > 0, "Llm:MaxTokens must be greater than zero.")
            .ValidateOnStart();

        services.AddOptions<ChatOptions>()
            .BindConfiguration(ChatOptions.SectionName)
            .Validate(options => options.MaxAttachmentsPerMessage > 0, "Chat:MaxAttachmentsPerMessage must be greater than zero.")
            .Validate(options => options.MaxAttachmentSizeMb > 0, "Chat:MaxAttachmentSizeMb must be greater than zero.")
            .Validate(options => options.AllowedAttachmentExtensions.Length > 0, "Chat:AllowedAttachmentExtensions must not be empty.")
            .ValidateOnStart();

        services.AddOptions<DocumentsOptions>()
            .BindConfiguration(DocumentsOptions.SectionName)
            .Validate(options => options.MaxSizeMb > 0, "Documents:MaxSizeMb must be greater than zero.")
            .Validate(options => options.AllowedExtensions.Length > 0, "Documents:AllowedExtensions must not be empty.")
            .ValidateOnStart();

        services.AddOptions<PaginationOptions>()
            .BindConfiguration(PaginationOptions.SectionName)
            .Validate(options => options.DefaultPageSize > 0, "Pagination:DefaultPageSize must be greater than zero.")
            .Validate(options => options.MaxPageSize >= options.DefaultPageSize, "Pagination:MaxPageSize must not be lower than Pagination:DefaultPageSize.")
            .ValidateOnStart();

        services.AddOptions<ReportingOptions>()
            .BindConfiguration(ReportingOptions.SectionName)
            .Validate(options => TimeZoneInfo.TryFindSystemTimeZoneById(options.TimeZone, out _), "Reporting:TimeZone must be a time zone known to the system.")
            .ValidateOnStart();

        services.AddOptions<UserActivityOptions>()
            .BindConfiguration(UserActivityOptions.SectionName)
            .Validate(options => options.MaxDays >= 7, "Reporting:UserActivity:MaxDays must be at least 7.")
            .Validate(options => options.DefaultDays >= 1 && options.DefaultDays <= options.MaxDays, "Reporting:UserActivity:DefaultDays must be between 1 and Reporting:UserActivity:MaxDays.")
            .Validate(options => options.SessionIntervalMinutes > 0, "Reporting:UserActivity:SessionIntervalMinutes must be greater than zero.")
            .ValidateOnStart();

        services.AddOptions<QuestionActivityOptions>()
            .BindConfiguration(QuestionActivityOptions.SectionName)
            .Validate(options => options.MaxWeeks >= 4, "Reporting:QuestionActivity:MaxWeeks must be at least 4.")
            .Validate(options => options.DefaultWeeks >= 1 && options.DefaultWeeks <= options.MaxWeeks, "Reporting:QuestionActivity:DefaultWeeks must be between 1 and Reporting:QuestionActivity:MaxWeeks.")
            .ValidateOnStart();

        services.AddOptions<AnswerFeedbackOptions>()
            .BindConfiguration(AnswerFeedbackOptions.SectionName)
            .Validate(options => options.UnhelpfulQuestionsLimit >= 1, "Reporting:AnswerFeedback:UnhelpfulQuestionsLimit must be at least 1.")
            .ValidateOnStart();

        services.AddOptions<IconsOptions>()
            .BindConfiguration(IconsOptions.SectionName)
            .Validate(options => options.MaxSizeMb > 0, "Icons:MaxSizeMb must be greater than zero.")
            .Validate(options => options.ContentTypeExtensions.Count > 0, "Icons:ContentTypeExtensions must not be empty.")
            .ValidateOnStart();

        services.Scan(scan => scan
            .FromAssemblies(AssemblyHelper.Domain)
            .AddClasses(classes => classes.AssignableTo(typeof(NhRepositoryBase<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddSingleton<IChatbotDocumentStorage>(sp =>
        {
            StorageOptions options = sp.GetRequiredService<IOptions<StorageOptions>>().Value;
            IWebHostEnvironment env = sp.GetRequiredService<IWebHostEnvironment>();
            string basePath = FileStorage.ResolvePath(options.ChatbotDocumentsPath, env);
            DocumentsOptions documentsOptions = sp.GetRequiredService<IOptions<DocumentsOptions>>().Value;
            return new DiskChatbotDocumentStorage(basePath, documentsOptions);
        });

        services.AddSingleton<IConversationAttachmentStorage>(sp =>
        {
            StorageOptions options = sp.GetRequiredService<IOptions<StorageOptions>>().Value;
            IWebHostEnvironment env = sp.GetRequiredService<IWebHostEnvironment>();
            string basePath = FileStorage.ResolvePath(options.ConversationAttachmentsPath, env);
            ChatOptions chatOptions = sp.GetRequiredService<IOptions<ChatOptions>>().Value;
            return new DiskConversationAttachmentStorage(basePath, chatOptions);
        });

        services.AddSingleton<IChatbotDocumentFileReader>(sp =>
        {
            StorageOptions options = sp.GetRequiredService<IOptions<StorageOptions>>().Value;
            IWebHostEnvironment env = sp.GetRequiredService<IWebHostEnvironment>();
            string basePath = FileStorage.ResolvePath(options.ChatbotDocumentsPath, env);
            return new DiskChatbotDocumentFileReader(basePath);
        });

        services.AddSingleton<IChatbotIconStorage>(sp =>
        {
            StorageOptions options = sp.GetRequiredService<IOptions<StorageOptions>>().Value;
            IWebHostEnvironment env = sp.GetRequiredService<IWebHostEnvironment>();
            string basePath = FileStorage.ResolvePath(options.ChatbotIconsPath, env);
            IconsOptions iconsOptions = sp.GetRequiredService<IOptions<IconsOptions>>().Value;
            return new DiskChatbotIconStorage(basePath, iconsOptions);
        });

        services.AddSingleton<IFormationIconStorage>(sp =>
        {
            StorageOptions options = sp.GetRequiredService<IOptions<StorageOptions>>().Value;
            IWebHostEnvironment env = sp.GetRequiredService<IWebHostEnvironment>();
            string basePath = FileStorage.ResolvePath(options.FormationIconsPath, env);
            IconsOptions iconsOptions = sp.GetRequiredService<IOptions<IconsOptions>>().Value;
            return new DiskFormationIconStorage(basePath, iconsOptions);
        });

        services.AddSingleton<ISessionFactory, NhSessionFactory>();
        services.AddSingleton<INhUnitOfWorkFactory, NhUnitOfWorkFactory>();
        services.AddScoped<NhUnitOfWork>();
        services.AddScoped<INhUnitOfWork>(sp => sp.GetRequiredService<NhUnitOfWork>());
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<NhUnitOfWork>());

        services.AddIdentityCore<User>()
            .AddUserStore<NhUserRepository>();

        services.AddKeyedScoped<ILlmClient, AnthropicLlmClient>(LlmProvider.Anthropic.ToString());
        services.AddSingleton<ISystemClock, SystemClock>();
        services.AddSingleton<IReportingCalendar, ReportingCalendar>();

        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(AssemblyHelper.Domain));

        services.Scan(scan => scan
            .FromAssemblies(AssemblyHelper.Domain)
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo<IService>())
            .AsSelf()
            .WithScopedLifetime());

        services.AddScoped<IEventBus, EventBus>();
        services.AddScoped<ICommandBus, CommandBus>();
        services.AddScoped<IQueryBus, QueryBus>();

        return services;
    }
}
