using System.Linq.Expressions;
using Nakama.Shared.Domain.Specification;

namespace Nakama.UserActivity.Domain;

public sealed class UserActivityBetweenSpecification : BaseSpecification<UserActivityEntry>
{
    public UserActivityBetweenSpecification(DateTime fromInclusive, DateTime toExclusive)
    {
        this.FromInclusive = fromInclusive;
        this.ToExclusive = toExclusive;
        this.ApplyOrderBy(entry => entry.OccurredAt);
    }

    public DateTime FromInclusive { get; }

    public DateTime ToExclusive { get; }

    protected override Expression<Func<UserActivityEntry, bool>> CreateExpression() =>
        entry => entry.OccurredAt >= this.FromInclusive && entry.OccurredAt < this.ToExclusive;
}
