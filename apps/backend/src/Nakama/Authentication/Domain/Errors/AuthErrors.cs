using Nakama.Shared.Domain;

namespace Nakama.Authentication.Domain.Errors;

public sealed class AuthInvalidRefreshTokenError()
    : DomainError("nakama.auth.invalid_refresh_token", "The refresh token is invalid, expired or revoked.");

public sealed class AuthRefreshTokenReusedError()
    : DomainError("nakama.auth.refresh_token_reused", "The refresh token was already used.");

public static class AuthErrors
{
    public static DomainError InvalidRefreshToken { get; } = new AuthInvalidRefreshTokenError();

    public static DomainError RefreshTokenReused { get; } = new AuthRefreshTokenReusedError();
}
