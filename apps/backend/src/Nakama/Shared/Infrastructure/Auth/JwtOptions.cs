namespace Nakama.Shared.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public const int MinSigningKeyBytes = 32;

    public required string Issuer { get; init; }

    public required string Audience { get; init; }

    public required string SigningKey { get; init; }

    public required int AccessTokenMinutes { get; init; }

    public required int RefreshTokenDays { get; init; }
}
