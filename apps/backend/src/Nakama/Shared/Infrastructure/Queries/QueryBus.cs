using Nakama.Shared.Domain;

namespace Nakama.Shared.Infrastructure.Queries;

public sealed class QueryBus(IServiceProvider serviceProvider) : IQueryBus
{
    public Task<TResponse> Ask<TResponse>(IQuery<TResponse> query, CancellationToken ct)
    {
        // IQueryHandler<TQuery, TResponse> is resolved directly from the container instead of
        // routing through MediatR - same reasoning as CommandBus (see ADR-13c). Reflection here
        // mirrors EventBus.WrapAsNotification: the concrete TQuery type is only known at
        // runtime, so the closed IQueryHandler<TQuery, TResponse> is built via MakeGenericType.
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
        var handler = serviceProvider.GetService(handlerType)
            ?? throw new InvalidOperationException($"No {handlerType} registered for {query.GetType()}.");
        var handleMethod = handlerType.GetMethod(nameof(IQueryHandler<IQuery<TResponse>, TResponse>.Handle))!;
        return (Task<TResponse>)handleMethod.Invoke(handler, [query, ct])!;
    }
}
