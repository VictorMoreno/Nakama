using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;
using Nakama.Users.Domain;
using Nakama.Users.Domain.Errors;

namespace Nakama.Users.Application.Authenticator;

public static class UserAuthenticator
{
    public sealed record Query(string Email, string Password) : IQuery<LoginResponse>;

    public sealed class Handler(Authenticator authenticator) : IQueryHandler<Query, LoginResponse>
    {
        public async Task<LoginResponse> Handle(Query query, CancellationToken ct)
        {
            Session session = await authenticator.Execute(new Email(query.Email), query.Password, ct);
            return new LoginResponse(session.AccessToken.Value, session.AccessToken.ExpiresAt, (UserResponse)session.User);
        }
    }

    public sealed record Session(User User, AccessToken AccessToken);

    public sealed class Authenticator(
        IUsersRepository repository,
        IEventBus eventBus,
        ISystemClock systemClock,
        ITokenIssuer tokenIssuer) : IService
    {
        public async Task<Session> Execute(Email email, string password, CancellationToken ct)
        {
            User? user = await repository.GetByEmail(email, ct);
            if (user is null || !await repository.VerifyPassword(user, password, ct))
            {
                throw UserErrors.InvalidCredentials;
            }

            user.MarkActive(systemClock.UtcNow);

            await repository.Save(user, ct);
            await eventBus.Publish(user, ct);

            return new Session(user, tokenIssuer.IssueAccessToken(user));
        }
    }
}
