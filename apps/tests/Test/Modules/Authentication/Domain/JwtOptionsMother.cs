using Nakama.Shared.Infrastructure.Auth;
using Test.Modules.Shared;

namespace Test.Modules.Authentication.Domain;

public static class JwtOptionsMother
{
    public static JwtOptions Random() => new()
    {
        Issuer = WordMother.Random(),
        Audience = WordMother.Random(),
        SigningKey = MotherCreator.Random.Random.AlphaNumeric(JwtOptions.MinSigningKeyBytes),
        AccessTokenMinutes = MotherCreator.Random.Random.Number(1, 60),
        RefreshTokenDays = MotherCreator.Random.Random.Number(1, 30)
    };
}
