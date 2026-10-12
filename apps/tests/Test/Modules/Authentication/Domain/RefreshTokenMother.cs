using Nakama.Authentication.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Authentication.Domain;

public static class RefreshTokenMother
{
    public static RefreshToken Create(
        RefreshTokenId id,
        Guid userId,
        string value,
        DateTime createdAt,
        DateTime expiresAt,
        DateTime? revokedAt,
        Guid? replacedById) =>
        new(id, userId, value, createdAt, expiresAt, revokedAt, replacedById);

    public static RefreshToken Random() => With();

    public static RefreshToken Expired() => With(expiresAt: DateTime.UtcNow.AddMinutes(-1));

    public static RefreshToken Revoked() => With(revokedAt: DateTime.UtcNow.AddMinutes(-1));

    public static RefreshToken Rotated() => With(replacedById: Guid.NewGuid());

    public static string RandomValue() => MotherCreator.Random.Random.AlphaNumeric(43);

    public static RefreshToken With(
        RefreshTokenId? id = null,
        Guid? userId = null,
        string? value = null,
        DateTime? createdAt = null,
        DateTime? expiresAt = null,
        DateTime? revokedAt = null,
        Guid? replacedById = null) =>
        Create(
            id ?? RefreshTokenId.New(),
            userId ?? Guid.NewGuid(),
            value ?? RandomValue(),
            createdAt ?? DateTime.UtcNow.AddMinutes(-5),
            expiresAt ?? DateTime.UtcNow.AddDays(7),
            revokedAt,
            replacedById);
}
