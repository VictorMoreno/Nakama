using NHibernate;

namespace Nakama.Shared.Infrastructure.Persistence;

public sealed class NhUnitOfWork(ISessionFactory sessionFactory) : INhUnitOfWork, IDisposable
{
    private ISession? _session;
    private ITransaction? _transaction;
    private bool _connectionReleased;

    public ISession Session
    {
        get
        {
            this._session ??= sessionFactory.OpenSession();

            if (this._connectionReleased)
            {
                this._session.Reconnect();
                this._connectionReleased = false;
            }

            return this._session;
        }
    }

    public bool HoldsConnection => this._session?.IsConnected ?? false;

    public void EnsureTransaction() => this._transaction ??= this.Session.BeginTransaction();

    public async Task<TResult> Read<TResult>(Func<ISession, Task<TResult>> read)
    {
        try
        {
            return await read(this.Session);
        }
        finally
        {
            this.ReleaseConnectionWhenIdle();
        }
    }

    public async Task SaveChanges(CancellationToken ct)
    {
        if (this._transaction is null)
        {
            return;
        }

        ITransaction transaction = this._transaction;
        this._transaction = null;

        using (transaction)
        {
            await transaction.CommitAsync(ct);
        }
    }

    public async Task Rollback(CancellationToken ct)
    {
        ITransaction? transaction = this._transaction;
        this._transaction = null;

        if (transaction is not null)
        {
            using (transaction)
            {
                await transaction.RollbackAsync(ct);
            }
        }

        this._session?.Clear();
    }

    public void Dispose()
    {
        this._transaction?.Dispose();
        this._session?.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        this.Dispose();

        return ValueTask.CompletedTask;
    }

    private void ReleaseConnectionWhenIdle()
    {
        if (this._transaction is not null || this._session is null || !this._session.IsConnected)
        {
            return;
        }

        this._session.Disconnect();
        this._connectionReleased = true;
    }
}
