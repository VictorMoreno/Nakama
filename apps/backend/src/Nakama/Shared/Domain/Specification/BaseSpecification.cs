using System.Linq.Expressions;

namespace Nakama.Shared.Domain.Specification;

public abstract class BaseSpecification<TEntity> : ISpecification<TEntity>
{
    private readonly List<OrderItem<TEntity>> _orders = [];

    public Expression<Func<TEntity, bool>> Criteria => this.CreateExpression();

    public IReadOnlyList<OrderItem<TEntity>> Orders => this._orders;

    public Expression<Func<TEntity, object>>? GroupBy { get; private set; }

    public int Take { get; private set; }

    public int Skip { get; private set; }

    public bool IsPagingEnabled { get; private set; }

    public static ISpecification<TEntity> operator &(BaseSpecification<TEntity> left, ISpecification<TEntity> right) =>
        left.And(right);

    public static ISpecification<TEntity> operator |(BaseSpecification<TEntity> left, ISpecification<TEntity> right) =>
        left.Or(right);

    public static ISpecification<TEntity> operator !(BaseSpecification<TEntity> specification) =>
        specification.Not();

    public bool IsSatisfiedBy(TEntity entity) => this.Criteria.Compile().Invoke(entity);

    public ISpecification<TEntity> And(ISpecification<TEntity> specification) =>
        new AndSpecification<TEntity>(this, specification);

    public ISpecification<TEntity> Or(ISpecification<TEntity> specification) =>
        new OrSpecification<TEntity>(this, specification);

    public ISpecification<TEntity> Not() => new NotSpecification<TEntity>(this);

    internal void InheritShape(ISpecification<TEntity> primary, ISpecification<TEntity> fallback)
    {
        ISpecification<TEntity> ordering = primary.Orders.Count > 0 ? primary : fallback;
        this._orders.AddRange(ordering.Orders);

        this.GroupBy = primary.GroupBy ?? fallback.GroupBy;

        ISpecification<TEntity> paging = primary.IsPagingEnabled ? primary : fallback;
        if (paging.IsPagingEnabled)
        {
            this.ApplyPaging(paging.Skip, paging.Take);
        }
    }

    protected abstract Expression<Func<TEntity, bool>> CreateExpression();

    protected virtual void ApplyPaging(int skip, int take)
    {
        this.Skip = skip;
        this.Take = take;
        this.IsPagingEnabled = true;
    }

    protected virtual List<OrderItem<TEntity>> ApplyOrderBy(Expression<Func<TEntity, object>> orderByExpression)
    {
        this._orders.Add(new OrderItem<TEntity>(orderByExpression, OrderType.Ascending));

        return this._orders;
    }

    protected virtual List<OrderItem<TEntity>> ApplyOrderByDescending(
        Expression<Func<TEntity, object>> orderByDescendingExpression)
    {
        this._orders.Add(new OrderItem<TEntity>(orderByDescendingExpression, OrderType.Descending));

        return this._orders;
    }

    protected virtual void ApplyGroupBy(Expression<Func<TEntity, object>> groupByExpression) =>
        this.GroupBy = groupByExpression;
}
