using NHibernate.Linq;

namespace Nakama.Shared.Infrastructure.Persistence.Nh;

/// <summary>
/// Centralizes the NHibernate session/transaction mechanics repeated across every
/// <c>Nh...Repository</c> - see agents.md §4/§5. Takes the shared <see cref="ISessionFactory"/>
/// so it serves both write repos (<see cref="IWriteSessionFactory"/>) and read repos
/// (<see cref="IReadSessionFactory"/>) without duplicating this class per side.
/// <see cref="FindById"/>/<see cref="FindAllEntities"/> are <c>protected</c>, not
/// <c>public</c>: each concrete repo exposes its own domain-named method (<c>FindByKey</c>,
/// <c>FindByBotKey</c>, <c>FindSingleton</c>...) that one-line-delegates into these, so the
/// interface consumers see stays domain language, never the generic base.
/// </summary>
public abstract class NhRepositoryBase<TEntity, TId>(ISessionFactory sessions) where TEntity : class
{
    protected async Task<TEntity?> FindById(TId id, CancellationToken ct)
    {
        using var session = sessions.OpenSession();
        return await session.GetAsync<TEntity>(id, ct);
    }

    protected async Task<IReadOnlyCollection<TEntity>> FindAllEntities(CancellationToken ct)
    {
        using var session = sessions.OpenSession();
        return await session.Query<TEntity>().ToListAsync(ct);
    }

    public async Task Save(TEntity entity, CancellationToken ct)
    {
        using var session = sessions.OpenSession();
        using var tx = session.BeginTransaction();
        await session.SaveOrUpdateAsync(entity, ct);
        await tx.CommitAsync(ct);
    }

    public Task Update(TEntity entity, CancellationToken ct) => this.Save(entity, ct);

    public async Task Remove(TEntity entity, CancellationToken ct)
    {
        using var session = sessions.OpenSession();
        using var tx = session.BeginTransaction();
        await session.DeleteAsync(entity, ct);
        await tx.CommitAsync(ct);
    }
}
