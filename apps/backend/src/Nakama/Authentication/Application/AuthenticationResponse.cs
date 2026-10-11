using Nakama.Authentication.Domain;
using Nakama.Users.Application;

namespace Nakama.Authentication.Application;

public sealed record AuthenticationResponse(TokenResponse Token, string RefreshToken, DateTime RefreshTokenExpiresAt)
{
    public static AuthenticationResponse From(UserResponse user, Token token, RefreshToken refreshToken) => new(
        new TokenResponse(token.Value, token.ExpiresAt, user),
        refreshToken.Value!,
        refreshToken.ExpiresAt);
}

public sealed record TokenResponse(string AccessToken, DateTime ExpiresAt, UserResponse User);
