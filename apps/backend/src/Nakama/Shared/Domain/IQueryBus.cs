namespace Nakama.Shared.Domain;

public interface IQueryBus
{
    Task<TResponse> Ask<TResponse>(IQuery<TResponse> query, CancellationToken ct);
}
