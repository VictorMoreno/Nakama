using System.Linq.Expressions;
using Nakama.Shared.Domain.Specification;

namespace Nakama.FormationsList.Domain;

public sealed class FormationSummaryPageSpecification : BaseSpecification<FormationSummary>
{
    public FormationSummaryPageSpecification(
        IReadOnlyCollection<Guid>? ids,
        string? search,
        string? status,
        int skip = 0,
        int? take = null)
    {
        this.Ids = ids;
        this.Search = string.IsNullOrWhiteSpace(search) ? null : search.Trim().ToLowerInvariant();
        this.Status = string.IsNullOrWhiteSpace(status) ? null : status.Trim();
        this.ApplyOrderBy(formation => formation.Title);

        if (take is not null)
        {
            this.ApplyPaging(skip, take.Value);
        }
    }

    public IReadOnlyCollection<Guid>? Ids { get; }

    public string? Search { get; }

    public string? Status { get; }

    protected override Expression<Func<FormationSummary, bool>> CreateExpression()
    {
        Expression<Func<FormationSummary, bool>> criteria = formation => !formation.Archived;

        if (this.Ids is not null)
        {
            IReadOnlyCollection<Guid> ids = this.Ids;
            criteria = criteria.And(ids.Count == 0
                ? PredicateBuilder.New<FormationSummary>(false)
                : formation => ids.Contains(formation.Id));
        }

        if (this.Status is not null)
        {
            string status = this.Status;
            criteria = criteria.And(formation => formation.Status == status);
        }

        if (this.Search is not null)
        {
            string search = this.Search;
            criteria = criteria.And(formation =>
                formation.Title.ToLower().Contains(search)
                || formation.Audience.ToLower().Contains(search)
                || formation.Description.ToLower().Contains(search));
        }

        return criteria;
    }
}
