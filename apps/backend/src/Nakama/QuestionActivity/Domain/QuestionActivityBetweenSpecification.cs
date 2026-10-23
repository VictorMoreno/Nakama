using System.Linq.Expressions;
using Nakama.Shared.Domain.Specification;

namespace Nakama.QuestionActivity.Domain;

public sealed class QuestionActivityBetweenSpecification : BaseSpecification<QuestionActivityEntry>
{
    public QuestionActivityBetweenSpecification(DateTime fromInclusive, DateTime toExclusive)
    {
        this.FromInclusive = fromInclusive;
        this.ToExclusive = toExclusive;
        this.ApplyOrderBy(entry => entry.OccurredAt);
    }

    public DateTime FromInclusive { get; }

    public DateTime ToExclusive { get; }

    protected override Expression<Func<QuestionActivityEntry, bool>> CreateExpression() =>
        entry => entry.OccurredAt >= this.FromInclusive && entry.OccurredAt < this.ToExclusive;
}
