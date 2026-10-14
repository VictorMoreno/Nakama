using System.Linq.Expressions;

namespace Nakama.Shared.Domain.Specification;

public interface ISpecification<TEntity>
{
    Expression<Func<TEntity, bool>> Criteria { get; }

    IReadOnlyList<OrderItem<TEntity>> Orders { get; }

    Expression<Func<TEntity, object>>? GroupBy { get; }

    int Take { get; }

    int Skip { get; }

    bool IsPagingEnabled { get; }

    bool IsSatisfiedBy(TEntity entity);

    ISpecification<TEntity> And(ISpecification<TEntity> specification);

    ISpecification<TEntity> Or(ISpecification<TEntity> specification);

    ISpecification<TEntity> Not();
}
