using Nakama.Shared.Domain;

namespace Nakama.Authentication.Domain;

public partial class RefreshToken : AggregateRoot<RefreshTokenId>
{
    protected RefreshToken()
    {
    }

    public virtual Guid UserId { get; private set; }

    public virtual string TokenHash { get; private set; } = string.Empty;

    public virtual string? Value { get; private set; }

    public virtual DateTime ExpiresAt { get; private set; }

    public virtual DateTime CreatedAt { get; private set; }

    public virtual DateTime? RevokedAt { get; private set; }

    public virtual Guid? ReplacedById { get; private set; }
}
