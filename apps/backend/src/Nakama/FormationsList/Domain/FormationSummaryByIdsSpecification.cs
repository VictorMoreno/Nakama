using System.Linq.Expressions;
using Nakama.Shared.Domain.Specification;

namespace Nakama.FormationsList.Domain;

public sealed class FormationSummaryByIdsSpecification : BaseSpecification<FormationSummary>
{
    public FormationSummaryByIdsSpecification(IReadOnlyCollection<Guid> ids)
    {
        this.Ids = ids;
        this.ApplyOrderBy(formation => formation.Title);
    }

    public IReadOnlyCollection<Guid> Ids { get; }

    protected override Expression<Func<FormationSummary, bool>> CreateExpression()
    {
        if (this.Ids.Count == 0)
        {
            return PredicateBuilder.New<FormationSummary>(false);
        }

        return formation => this.Ids.Contains(formation.Id);
    }
}
