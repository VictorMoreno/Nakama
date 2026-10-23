using System.Linq.Expressions;
using Nakama.Shared.Domain.Specification;

namespace Nakama.UserActivity.Domain;

public sealed class UserActivityOfUserSinceSpecification(Guid userId, DateTime since) : BaseSpecification<UserActivityEntry>
{
    public Guid UserId { get; } = userId;

    public DateTime Since { get; } = since;

    protected override Expression<Func<UserActivityEntry, bool>> CreateExpression() =>
        entry => entry.UserId == this.UserId && entry.OccurredAt >= this.Since;
}
