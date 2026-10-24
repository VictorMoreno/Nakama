using System.Linq.Expressions;
using Nakama.Shared.Domain.Specification;

namespace Nakama.QuestionActivity.Domain;

public sealed class QuestionActivityBetweenSpecification : BaseSpecification<QuestionActivityEntry>
{
    public QuestionActivityBetweenSpecification(DateTime fromInclusive, DateTime toExclusive, Guid? userId = null)
    {
        this.UserId = userId;
        this.FromInclusive = fromInclusive;
        this.ToExclusive = toExclusive;
        this.ApplyOrderBy(entry => entry.OccurredAt);
    }

    public DateTime FromInclusive { get; }

    public DateTime ToExclusive { get; }

    public Guid? UserId { get; }

    protected override Expression<Func<QuestionActivityEntry, bool>> CreateExpression()
    {
        DateTime from = this.FromInclusive;
        DateTime to = this.ToExclusive;
        Expression<Func<QuestionActivityEntry, bool>> criteria = entry => entry.OccurredAt >= from && entry.OccurredAt < to;

        if (this.UserId is not null)
        {
            Guid userId = this.UserId.Value;
            criteria = criteria.And(entry => entry.UserId == userId);
        }

        return criteria;
    }
}
