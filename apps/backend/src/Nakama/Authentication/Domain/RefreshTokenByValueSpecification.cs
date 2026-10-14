using System.Linq.Expressions;
using Nakama.Shared.Domain.Specification;

namespace Nakama.Authentication.Domain;

public sealed class RefreshTokenByValueSpecification(string value) : BaseSpecification<RefreshToken>
{
    public string Value { get; } = value;

    public string TokenHash { get; } = RefreshToken.HashOf(value);

    protected override Expression<Func<RefreshToken, bool>> CreateExpression() => token => token.TokenHash == this.TokenHash;
}
