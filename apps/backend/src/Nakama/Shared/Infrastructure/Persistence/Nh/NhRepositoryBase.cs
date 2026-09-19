using NHibernate.Linq;

namespace Nakama.Shared.Infrastructure.Persistence.Nh;

public abstract class NhRepositoryBase<TEntity, TId>(ISessionFactory sessions) where TEntity : class
{
    public async Task<TEntity?> FindById(TId id, CancellationToken ct)
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
