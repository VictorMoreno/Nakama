using System.Linq.Expressions;
using Nakama.Shared.Domain.Specification;

namespace Nakama.Users.Domain;

public sealed class UserListingSpecification : BaseSpecification<User>
{
    public UserListingSpecification() => this.ApplyOrderBy(user => user.Name);

    protected override Expression<Func<User, bool>> CreateExpression() => PredicateBuilder.New<User>();
}
