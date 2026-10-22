using Nakama.Shared.Domain;
using Nakama.Shared.Domain.Specification;
using NHibernate;
using NHibernate.Linq;

namespace Nakama.Shared.Infrastructure.Persistence.Nh;

public abstract class NhRepositoryBase<TEntity, TId>(INhUnitOfWork unitOfWork) where TEntity : class
{
    protected INhUnitOfWork UnitOfWork => unitOfWork;

    public async Task<TEntity?> FindById(TId id, CancellationToken ct) =>
        await unitOfWork.Read(session => session.GetAsync<TEntity>(id, ct));

    public async Task<IReadOnlyList<TEntity>> Search(ISpecification<TEntity> specification, CancellationToken ct)
    {
        if (specification.GroupBy is null)
        {
            return await unitOfWork.Read<IReadOnlyList<TEntity>>(async session =>
                await NhSpecificationConverter<TEntity>.GetQuery(session.Query<TEntity>(), specification).ToListAsync(ct));
        }

        List<TEntity> ordered = await unitOfWork.Read(session => NhSpecificationConverter<TEntity>
            .GetQuery(session.Query<TEntity>(), specification, includePaging: false)
            .ToListAsync(ct));

        return NhSpecificationConverter<TEntity>.FirstOfEachGroup(ordered, specification);
    }

    public async Task<PagedResult<TEntity>> SearchPage(ISpecification<TEntity> specification, CancellationToken ct)
    {
        if (specification.GroupBy is not null)
        {
            IReadOnlyList<TEntity> groups = await this.Search(specification, ct);

            return new PagedResult<TEntity>(groups, await this.Count(specification, ct));
        }

        return await unitOfWork.Read(async session =>
        {
            IFutureValue<int> totalCount = session.Query<TEntity>().Where(specification.Criteria).ToFutureValue(query => query.Count());
            IFutureEnumerable<TEntity> page = NhSpecificationConverter<TEntity>.GetQuery(session.Query<TEntity>(), specification).ToFuture();

            List<TEntity> items = (await page.GetEnumerableAsync(ct)).ToList();

            return new PagedResult<TEntity>(items, await totalCount.GetValueAsync(ct));
        });
    }

    public async Task<TEntity> Find(ISpecification<TEntity> specification, CancellationToken ct)
    {
        if (specification.GroupBy is not null)
        {
            IReadOnlyList<TEntity> groups = await this.Search(specification, ct);

            return groups.First();
        }

        return await unitOfWork.Read(session =>
            NhSpecificationConverter<TEntity>.GetQuery(session.Query<TEntity>(), specification).FirstAsync(ct));
    }

    public async Task<int> Count(ISpecification<TEntity> specification, CancellationToken ct)
    {
        if (specification.GroupBy is null)
        {
            return await unitOfWork.Read(session => session.Query<TEntity>().Where(specification.Criteria).CountAsync(ct));
        }

        List<TEntity> entities = await unitOfWork.Read(session => session.Query<TEntity>().Where(specification.Criteria).ToListAsync(ct));

        return NhSpecificationConverter<TEntity>.CountGroups(entities, specification);
    }

    public async Task Save(TEntity entity, CancellationToken ct)
    {
        unitOfWork.EnsureTransaction();
        await unitOfWork.Session.SaveOrUpdateAsync(entity, ct);
    }

    public Task Update(TEntity entity, CancellationToken ct) => this.Save(entity, ct);

    public async Task Remove(TEntity entity, CancellationToken ct)
    {
        unitOfWork.EnsureTransaction();
        await unitOfWork.Session.DeleteAsync(entity, ct);
    }
}
