using System.Reflection;
using Nakama.Shared.Domain;

namespace Nakama.Shared.Infrastructure.Queries;

public sealed class QueryBus(IServiceProvider serviceProvider) : IQueryBus
{
    public Task<TResponse> Ask<TResponse>(IQuery<TResponse> query, CancellationToken ct)
    {
        Type handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
        object handler = serviceProvider.GetService(handlerType)
                         ?? throw new InvalidOperationException($"No {handlerType} registered for {query.GetType()}.");
        MethodInfo handleMethod = handlerType.GetMethod(nameof(IQueryHandler<IQuery<TResponse>, TResponse>.Handle))!;
        return (Task<TResponse>)handleMethod.Invoke(handler, [query, ct])!;
    }
}
