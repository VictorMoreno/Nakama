using System.Linq.Expressions;
using Nakama.Shared.Domain.Specification;

namespace Nakama.AnswerFeedback.Domain;

public sealed class UnhelpfulAnswersBetweenSpecification : BaseSpecification<AnswerFeedbackEntry>
{
    public UnhelpfulAnswersBetweenSpecification(DateTime fromInclusive, DateTime toExclusive)
    {
        this.FromInclusive = fromInclusive;
        this.ToExclusive = toExclusive;
        this.ApplyOrderBy(entry => entry.RatedAt);
    }

    public DateTime FromInclusive { get; }

    public DateTime ToExclusive { get; }

    protected override Expression<Func<AnswerFeedbackEntry, bool>> CreateExpression() =>
        entry => !entry.Useful && entry.RatedAt >= this.FromInclusive && entry.RatedAt < this.ToExclusive;
}
