using Api.Configuration.Swagger;
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

        services.AddHealthChecks();

        // Maps DomainError -> HTTP status/ProblemDetails globally (see ADR entry in
        // apps/backend/src/Nakama/ADR.md and agents.md) - no endpoint needs its own try/catch.
        services.AddExceptionHandler<DomainErrorExceptionHandler>();
        services.AddProblemDetails();

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

        // ICommandHandler<>/IQueryHandler<,> are the pure, MediatR-free contracts every command/
        // query use case implements (see ADR-13c) - CommandBus/QueryBus resolve them directly
        // from this container by reflection, MediatR is not involved. AddMediatR's scan above
        // only covers the domain-event side (IEventBus/DomainEventHandler<TEvent>, ADR-7), whose
        // concrete handlers are non-generic and need no special registration.
        services.Scan(scan => scan
            .FromAssemblyOf<Chatbot>()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        // ISessionFactory construction is expensive - singleton, built once, lazily on first
        // use (see NhWriteSessionFactory/NhReadSessionFactory), never per request.
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