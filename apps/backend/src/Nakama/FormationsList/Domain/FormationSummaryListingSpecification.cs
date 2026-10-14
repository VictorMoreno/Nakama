using System.Linq.Expressions;
using Nakama.Shared.Domain.Specification;

namespace Nakama.FormationsList.Domain;

public sealed class FormationSummaryListingSpecification : BaseSpecification<FormationSummary>
{
    public FormationSummaryListingSpecification() => this.ApplyOrderBy(formation => formation.Title);

    protected override Expression<Func<FormationSummary, bool>> CreateExpression() => PredicateBuilder.New<FormationSummary>();
}
