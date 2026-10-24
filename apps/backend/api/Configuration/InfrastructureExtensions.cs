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
using Nakama.Shared.Infrastructure.Documents;
using Nakama.Shared.Infrastructure.Icons;
using Nakama.Shared.Infrastructure.Commands;
using Nakama.Shared.Infrastructure.Events;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;
using Nakama.Shared.Infrastructure.Queries;
using Nakama.Shared.Infrastructure.Storage;
using Nakama.Shared.Infrastructure.Reporting;
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
            .AddEndpoints()
            .AddOptionsConfiguration();

        services.AddHealthChecks()
            .AddNpgSql(
                sp => sp.GetRequiredService<IConfiguration>().GetConnectionString("Nakama")!,
                name: "postgres");

        services.AddExceptionHandler<ApiExceptionHandler>();
        services.AddProblemDetails();

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

        services.AddSingleton<IFormationDocumentStorage>(sp =>
        {
            StorageOptions options = sp.GetRequiredService<IOptions<StorageOptions>>().Value;
            IWebHostEnvironment env = sp.GetRequiredService<IWebHostEnvironment>();
            string basePath = FileStorage.ResolvePath(options.FormationDocumentsPath, env);
            DocumentsOptions documentsOptions = sp.GetRequiredService<IOptions<DocumentsOptions>>().Value;
            return new DiskFormationDocumentStorage(basePath, documentsOptions);
        });

        services.AddSingleton<IFormationDocumentFileReader>(sp =>
        {
            StorageOptions options = sp.GetRequiredService<IOptions<StorageOptions>>().Value;
            IWebHostEnvironment env = sp.GetRequiredService<IWebHostEnvironment>();
            string basePath = FileStorage.ResolvePath(options.FormationDocumentsPath, env);
            return new DiskFormationDocumentFileReader(basePath);
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
