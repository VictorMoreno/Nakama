using Nakama.Shared.Domain;

namespace Nakama.Authentication.Domain;

public partial class RefreshToken : AggregateRoot<RefreshTokenId>
{
    protected RefreshToken()
    {
    }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public string? Value { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? RevokedAt { get; private set; }

    public Guid? ReplacedById { get; private set; }
}
