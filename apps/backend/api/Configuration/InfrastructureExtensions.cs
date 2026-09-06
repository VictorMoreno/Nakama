using Api.Configuration.Swagger;
using MediatR;
using Nakama.Analytics.Domain;
using Nakama.Analytics.Infrastructure;
using Nakama.ChatbotDetail.Domain;
using Nakama.ChatbotDetail.Infrastructure;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Infrastructure;
using Nakama.ChatbotsList.Domain;
using Nakama.ChatbotsList.Infrastructure;
using Nakama.ChatPage.Domain;
using Nakama.ChatPage.Infrastructure;
using Nakama.FormationDetail.Domain;
using Nakama.FormationDetail.Infrastructure;
using Nakama.Formations.Domain;
using Nakama.Formations.Infrastructure;
using Nakama.FormationsList.Domain;
using Nakama.FormationsList.Infrastructure;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure;
using Nakama.Shared.Infrastructure.Events;
using Nakama.Shared.Infrastructure.Persistence;

namespace Api.Configuration;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSwagger()
            .AddEndpoints();

        services.AddHealthChecks();

        return services;
    }

    /// <summary>
    /// Wires MediatR (scanning the Nakama assembly for the use-case Handlers and read-model
    /// Projections it also owns) plus the write/read NHibernate session factories, per the
    /// plan §6. Database-per-tenant resolution is explicitly deferred - the write/read
    /// <see cref="ISessionFactory"/> registrations below use a single placeholder connection
    /// string from configuration for now.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Chatbot).Assembly));

        // ISessionFactory construction is expensive - singleton, built once, lazily on first
        // use (see NhWriteSessionFactory/NhReadSessionFactory), never per request.
        services.AddSingleton<IWriteSessionFactory, NhWriteSessionFactory>();
        services.AddSingleton<IReadSessionFactory, NhReadSessionFactory>();

        services.AddSingleton<ISystemClock, SystemClock>();
        services.AddSingleton<IUuidGenerator, GuidUuidGenerator>();
        services.AddScoped<IEventBus, EventBus>();

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