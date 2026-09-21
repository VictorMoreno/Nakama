using Api.Configuration.Swagger;
using Nakama.Analytics.Domain;
using Nakama.Analytics.Infrastructure;
using Nakama.ChatbotDetail.Domain;
using Nakama.ChatbotDetail.Infrastructure;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Infrastructure;
using Nakama.ChatbotSummaries.Domain;
using Nakama.ChatbotSummaries.Infrastructure;
using Nakama.ChatPage.Domain;
using Nakama.ChatPage.Infrastructure;
using Nakama.FormationDetail.Domain;
using Nakama.FormationDetail.Infrastructure;
using Nakama.Formations.Domain;
using Nakama.Formations.Infrastructure;
using Nakama.FormationsList.Domain;
using Nakama.FormationsList.Infrastructure;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure;
using Nakama.Shared.Infrastructure.Commands;
using Nakama.Shared.Infrastructure.Events;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Queries;

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

        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Chatbot).Assembly));
        
        services.Scan(scan => scan
            .FromAssemblyOf<Chatbot>()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo<IService>())
                .AsSelf()
                .WithScopedLifetime());

        services.AddSingleton<IWriteSessionFactory, NhWriteSessionFactory>();
        services.AddSingleton<IReadSessionFactory, NhReadSessionFactory>();

        services.AddSingleton<ISystemClock, SystemClock>();
        services.AddScoped<IEventBus, EventBus>();
        services.AddScoped<ICommandBus, CommandBus>();
        services.AddScoped<IQueryBus, QueryBus>();

        services.AddScoped<IChatbotsRepository, NhChatbotsRepository>();
        services.AddScoped<IConversationsRepository, NhConversationsRepository>();
        services.AddScoped<IFormationsRepository, NhFormationsRepository>();

        // Read-model modules - each backed by its own repository over the read session factory.
        services.AddScoped<IChatbotSummaryRepository, NhChatbotSummaryRepository>();
        services.AddScoped<IChatbotDetailRepository, NhChatbotDetailRepository>();
        services.AddScoped<IChatPageRepository, NhChatPageRepository>();
        services.AddScoped<IFormationSummaryRepository, NhFormationSummaryRepository>();
        services.AddScoped<IFormationDetailRepository, NhFormationDetailRepository>();
        services.AddScoped<IChatbotLabelRefRepository, NhChatbotLabelRefRepository>();
        services.AddScoped<IAnalyticsRepository, NhAnalyticsRepository>();

        return services;
    }
}