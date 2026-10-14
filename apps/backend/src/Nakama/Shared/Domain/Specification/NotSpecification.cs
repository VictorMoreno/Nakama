using System.Linq.Expressions;

namespace Nakama.Shared.Domain.Specification;

internal sealed class NotSpecification<TEntity> : BaseSpecification<TEntity>
{
    private readonly ISpecification<TEntity> _specification;

    public NotSpecification(ISpecification<TEntity> specification)
    {
        this._specification = specification;
        this.InheritShape(specification, specification);
    }

    protected override Expression<Func<TEntity, bool>> CreateExpression() => this._specification.Criteria.Not();
}
