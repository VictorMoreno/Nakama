using Nakama.Authentication.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Authentication.Domain;

public static class TokenMother
{
    public static Token Random() => With();

    public static Token With(string? value = null, DateTime? expiresAt = null) =>
        new(
            value ?? MotherCreator.Random.Random.AlphaNumeric(40),
            expiresAt ?? DateTime.UtcNow.AddMinutes(15));
}
