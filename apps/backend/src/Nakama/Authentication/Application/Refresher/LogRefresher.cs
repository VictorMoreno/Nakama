using Microsoft.Extensions.Options;
using Nakama.Authentication.Domain;
using Nakama.Authentication.Domain.Errors;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Auth;
using Nakama.Users.Application;
using Nakama.Users.Application.Finder;

namespace Nakama.Authentication.Application.Refresher;

public static class LogRefresher
{
    public sealed record Query(string? RefreshToken) : IQuery<AuthenticationResponse>;

    public sealed class Handler(Refresher refresher) : IQueryHandler<Query, AuthenticationResponse>
    {
        public Task<AuthenticationResponse> Handle(Query query, CancellationToken ct) =>
            refresher.Execute(query.RefreshToken, ct);
    }

    public sealed class Refresher(
        IQueryBus queryBus,
        IRefreshTokensRepository refreshTokens,
        ISystemClock systemClock,
        ITokenGenerator tokenGenerator,
        IOptions<JwtOptions> jwtOptions) : IService
    {
        public async Task<AuthenticationResponse> Execute(string? refreshToken, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw AuthErrors.InvalidRefreshToken;
            }

            DateTime now = systemClock.UtcNow;

            IReadOnlyList<RefreshToken> found = await refreshTokens.Search(new RefreshTokenByValueSpecification(refreshToken), ct);
            RefreshToken current = found.FirstOrDefault() ?? throw AuthErrors.InvalidRefreshToken;

            await this.EnsureValid(current, refreshToken, now, ct);

            UserResponse user = await this.FindUser(current.UserId, ct);

            Token token = tokenGenerator.Generate(user.Id, user.Email, user.Name, user.IsAdmin);
            RefreshToken replacement = RefreshToken.Create(user.Id, systemClock, TimeSpan.FromDays(jwtOptions.Value.RefreshTokenDays));

            if (!await refreshTokens.Rotate(current, replacement, now, ct))
            {
                await refreshTokens.RevokeAllOf(current.UserId, now, ct);
                throw AuthErrors.InvalidRefreshToken;
            }

            return AuthenticationResponse.From(user, token, replacement);
        }

        private async Task EnsureValid(RefreshToken current, string refreshToken, DateTime now, CancellationToken ct)
        {
            try
            {
                current.Validate(refreshToken, now);
            }
            catch (AuthRefreshTokenReusedError)
            {
                await refreshTokens.RevokeAllOf(current.UserId, now, ct);
                throw AuthErrors.InvalidRefreshToken;
            }
        }

        private async Task<UserResponse> FindUser(Guid userId, CancellationToken ct)
        {
            try
            {
                return await queryBus.Ask(new UserFinder.Query(userId), ct);
            }
            catch (DomainError)
            {
                throw AuthErrors.InvalidRefreshToken;
            }
        }
    }
}
