using System.Linq.Expressions;
using Nakama.Shared.Domain.Specification;

namespace Nakama.UserActivity.Domain;

public sealed class UserActivityBetweenSpecification : BaseSpecification<UserActivityEntry>
{
    public UserActivityBetweenSpecification(DateTime fromInclusive, DateTime toExclusive, Guid? userId = null)
    {
        this.UserId = userId;
        this.FromInclusive = fromInclusive;
        this.ToExclusive = toExclusive;
        this.ApplyOrderBy(entry => entry.OccurredAt);
    }

    public DateTime FromInclusive { get; }

    public DateTime ToExclusive { get; }

    public Guid? UserId { get; }

    protected override Expression<Func<UserActivityEntry, bool>> CreateExpression()
    {
        DateTime from = this.FromInclusive;
        DateTime to = this.ToExclusive;
        Expression<Func<UserActivityEntry, bool>> criteria = entry => entry.OccurredAt >= from && entry.OccurredAt < to;

        if (this.UserId is not null)
        {
            Guid userId = this.UserId.Value;
            criteria = criteria.And(entry => entry.UserId == userId);
        }

        return criteria;
    }
}
