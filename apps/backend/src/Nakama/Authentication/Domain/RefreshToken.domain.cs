using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using Nakama.Authentication.Domain.Errors;
using Nakama.Shared.Domain;

namespace Nakama.Authentication.Domain;

public partial class RefreshToken
{
    private const int ValueSizeInBytes = 32;

    public RefreshToken(
        RefreshTokenId key,
        Guid userId,
        string tokenHash,
        string? value,
        DateTime createdAt,
        DateTime expiresAt,
        DateTime? revokedAt,
        Guid? replacedById)
    {
        this.Id = key;
        this.UserId = userId;
        this.TokenHash = tokenHash;
        this.Value = value;
        this.CreatedAt = createdAt;
        this.ExpiresAt = expiresAt;
        this.RevokedAt = revokedAt;
        this.ReplacedById = replacedById;
    }

    public static RefreshToken Create(Guid userId, ISystemClock systemClock, TimeSpan lifetime)
    {
        string value = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(ValueSizeInBytes));
        DateTime now = systemClock.UtcNow;

        return new RefreshToken(RefreshTokenId.New(), userId, HashOf(value), value, now, now.Add(lifetime), null, null);
    }

    public static string HashOf(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    public void Validate(string value, DateTime now)
    {
        if (!this.Matches(value))
        {
            throw AuthErrors.InvalidRefreshToken;
        }

        if (this.ReplacedById is not null)
        {
            throw AuthErrors.RefreshTokenReused;
        }

        if (this.RevokedAt is not null || this.ExpiresAt <= now)
        {
            throw AuthErrors.InvalidRefreshToken;
        }
    }

    public void Revoke(DateTime now) => this.RevokedAt ??= now;

    private bool Matches(string value) =>
        CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(this.TokenHash), Encoding.UTF8.GetBytes(HashOf(value)));
}
