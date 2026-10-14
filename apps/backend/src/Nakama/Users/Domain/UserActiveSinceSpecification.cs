using System.Linq.Expressions;
using Nakama.Shared.Domain.Specification;

namespace Nakama.Users.Domain;

public sealed class UserActiveSinceSpecification(DateTime cutoff) : BaseSpecification<User>
{
    public DateTime Cutoff { get; } = cutoff;

    protected override Expression<Func<User, bool>> CreateExpression() =>
        user => user.LastActiveAt != null && user.LastActiveAt >= this.Cutoff;
}
