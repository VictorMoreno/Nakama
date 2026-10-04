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
using Nakama.Shared.Infrastructure.Commands;
using Nakama.Shared.Infrastructure.Events;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;
using Nakama.Shared.Infrastructure.Queries;
using Nakama.Shared.Infrastructure.Storage;
using Nakama.Users.Domain;
using Nakama.Users.Infrastructure;

namespace Api.Configuration;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSwagger()
            .AddEndpoints();

        services.AddHealthChecks()
            .AddNpgSql(
                sp => sp.GetRequiredService<IConfiguration>().GetConnectionString("Nakama")!,
                name: "postgres");

        services.AddExceptionHandler<DomainErrorExceptionHandler>();
        services.AddProblemDetails();

        services.AddOptions<StorageOptions>()
            .BindConfiguration(StorageOptions.SectionName)
            .ValidateOnStart();

        services.Scan(scan => scan
            .FromAssemblies(AssemblyHelper.Domain)
            .AddClasses(classes => classes.AssignableTo(typeof(NhRepositoryBase<,>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddSingleton<IChatbotDocumentStorage>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<StorageOptions>>().Value;
            var env = sp.GetRequiredService<IWebHostEnvironment>();
            var basePath = FileStorage.ResolvePath(options.ChatbotDocumentsPath, env);
            return new DiskChatbotDocumentStorage(basePath);
        });

        services.AddSingleton<IChatbotIconStorage>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<StorageOptions>>().Value;
            var env = sp.GetRequiredService<IWebHostEnvironment>();
            var basePath = FileStorage.ResolvePath(options.ChatbotIconsPath, env);
            return new DiskChatbotIconStorage(basePath);
        });

        services.AddSingleton<IFormationIconStorage>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<StorageOptions>>().Value;
            var env = sp.GetRequiredService<IWebHostEnvironment>();
            var basePath = FileStorage.ResolvePath(options.FormationIconsPath, env);
            return new DiskFormationIconStorage(basePath);
        });

        services.AddSingleton<IWriteSessionFactory, NhWriteSessionFactory>();
        services.AddSingleton<IReadSessionFactory, NhReadSessionFactory>();

        services.AddIdentityCore<User>()
            .AddUserStore<NhUserStore>();

        services.AddKeyedScoped<ILlmClient, AnthropicLlmClient>(LlmProvider.Anthropic.ToString());
        services.AddScoped<IChatbotDocumentContentReader, ChatbotDocumentContentReader>();
        services.AddSingleton<ISystemClock, SystemClock>();

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
