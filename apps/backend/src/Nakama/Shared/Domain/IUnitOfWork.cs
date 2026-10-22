namespace Nakama.Shared.Domain;

public interface IUnitOfWork : IAsyncDisposable
{
    Task SaveChanges(CancellationToken ct);

    Task Rollback(CancellationToken ct);
}
