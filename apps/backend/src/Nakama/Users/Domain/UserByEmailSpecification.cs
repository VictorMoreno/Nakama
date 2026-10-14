using System.Linq.Expressions;
using Nakama.Shared.Domain.Specification;

namespace Nakama.Users.Domain;

public sealed class UserByEmailSpecification(Email email) : BaseSpecification<User>
{
    public Email Email { get; } = email;

    protected override Expression<Func<User, bool>> CreateExpression() =>
        user => user.NormalizedEmail == this.Email.Normalized;
}
