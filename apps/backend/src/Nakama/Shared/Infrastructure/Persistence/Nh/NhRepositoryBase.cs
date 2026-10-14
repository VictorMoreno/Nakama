using Nakama.Shared.Domain.Specification;
using NHibernate;
using NHibernate.Linq;

namespace Nakama.Shared.Infrastructure.Persistence.Nh;

public abstract class NhRepositoryBase<TEntity, TId>(ISessionFactory sessions) where TEntity : class
{
    protected ISessionFactory Sessions => sessions;

    public async Task<TEntity?> FindById(TId id, CancellationToken ct)
    {
        using ISession session = sessions.OpenSession();
        return await session.GetAsync<TEntity>(id, ct);
    }

    protected async Task<IReadOnlyCollection<TEntity>> FindAllEntities(CancellationToken ct)
    {
        using ISession session = sessions.OpenSession();
        return await session.Query<TEntity>().ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TEntity>> Search(ISpecification<TEntity> specification, CancellationToken ct)
    {
        using ISession session = sessions.OpenSession();

        if (specification.GroupBy is null)
        {
            return await NhSpecificationConverter<TEntity>.GetQuery(session.Query<TEntity>(), specification).ToListAsync(ct);
        }

        List<TEntity> ordered = await NhSpecificationConverter<TEntity>
            .GetQuery(session.Query<TEntity>(), specification, includePaging: false)
            .ToListAsync(ct);

        return NhSpecificationConverter<TEntity>.FirstOfEachGroup(ordered, specification);
    }

    public async Task<TEntity> Find(ISpecification<TEntity> specification, CancellationToken ct)
    {
        if (specification.GroupBy is not null)
        {
            IReadOnlyList<TEntity> groups = await this.Search(specification, ct);

            return groups.First();
        }

        using ISession session = sessions.OpenSession();

        return await NhSpecificationConverter<TEntity>.GetQuery(session.Query<TEntity>(), specification).FirstAsync(ct);
    }

    public async Task<int> Count(ISpecification<TEntity> specification, CancellationToken ct)
    {
        using ISession session = sessions.OpenSession();
        IQueryable<TEntity> matching = session.Query<TEntity>().Where(specification.Criteria);

        if (specification.GroupBy is null)
        {
            return await matching.CountAsync(ct);
        }

        List<TEntity> entities = await matching.ToListAsync(ct);

        return NhSpecificationConverter<TEntity>.CountGroups(entities, specification);
    }

    public async Task Save(TEntity entity, CancellationToken ct)
    {
        using ISession session = sessions.OpenSession();
        using ITransaction tx = session.BeginTransaction();
        await session.SaveOrUpdateAsync(entity, ct);
        await tx.CommitAsync(ct);
    }

    public Task Update(TEntity entity, CancellationToken ct) => this.Save(entity, ct);

    public async Task Remove(TEntity entity, CancellationToken ct)
    {
        using ISession session = sessions.OpenSession();
        using ITransaction tx = session.BeginTransaction();
        await session.DeleteAsync(entity, ct);
        await tx.CommitAsync(ct);
    }
}
