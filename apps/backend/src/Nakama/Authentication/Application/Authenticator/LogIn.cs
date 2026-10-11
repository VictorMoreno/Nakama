using Microsoft.Extensions.Options;
using Nakama.Authentication.Domain;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Auth;
using Nakama.Users.Application;
using Nakama.Users.Application.CredentialsVerifier;

namespace Nakama.Authentication.Application.Authenticator;

public static class LogIn
{
    public sealed record Query(string Email, string Password) : IQuery<AuthenticationResponse>;

    public sealed class Handler(Authenticator authenticator) : IQueryHandler<Query, AuthenticationResponse>
    {
        public Task<AuthenticationResponse> Handle(Query query, CancellationToken ct) =>
            authenticator.Execute(query.Email, query.Password, ct);
    }

    public sealed class Authenticator(
        IQueryBus queryBus,
        IRefreshTokensRepository refreshTokens,
        ISystemClock systemClock,
        ITokenGenerator tokenGenerator,
        IOptions<JwtOptions> jwtOptions) : IService
    {
        public async Task<AuthenticationResponse> Execute(string email, string password, CancellationToken ct)
        {
            UserResponse user = await queryBus.Ask(new UserCredentialsVerifier.Query(email, password), ct);

            Token token = tokenGenerator.Generate(user.Id, user.Email, user.Name, user.IsAdmin);
            RefreshToken refreshToken = RefreshToken.Create(user.Id, systemClock, TimeSpan.FromDays(jwtOptions.Value.RefreshTokenDays));

            await refreshTokens.Save(refreshToken, ct);

            return AuthenticationResponse.From(user, token, refreshToken);
        }
    }
}
