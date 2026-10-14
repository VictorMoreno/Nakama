using System.Linq.Expressions;

namespace Nakama.Shared.Domain.Specification;

internal sealed class OrSpecification<TEntity> : BaseSpecification<TEntity>
{
    private readonly ISpecification<TEntity> _left;

    private readonly ISpecification<TEntity> _right;

    public OrSpecification(ISpecification<TEntity> left, ISpecification<TEntity> right)
    {
        this._left = left;
        this._right = right;
        this.InheritShape(left, right);
    }

    protected override Expression<Func<TEntity, bool>> CreateExpression() =>
        this._left.Criteria.Or(this._right.Criteria);
}
