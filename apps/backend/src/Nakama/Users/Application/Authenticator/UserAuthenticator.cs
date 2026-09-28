using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;
using Nakama.Users.Domain;
using Nakama.Users.Domain.Errors;

namespace Nakama.Users.Application.Authenticator;

public static class UserAuthenticator
{
    public sealed record Query(string Email, string Password) : IQuery<UserResponse>;

    public sealed class Handler(Authenticator authenticator) : IQueryHandler<Query, UserResponse>
    {
        public async Task<UserResponse> Handle(Query query, CancellationToken ct)
        {
            var user = await authenticator.Execute(new Email(query.Email), query.Password, ct);
            return (UserResponse)user;
        }
    }

    public sealed class Authenticator(
        IUsersRepository repository,
        IEventBus eventBus,
        ISystemClock systemClock) : IService
    {
        public async Task<User> Execute(Email email, string password, CancellationToken ct)
        {
            var user = await repository.GetByEmail(email, ct);
            if (user is null || !await repository.VerifyPassword(user, password, ct))
            {
                throw UserErrors.InvalidCredentials;
            }

            user.MarkActive(systemClock.UtcNow);
            await repository.Save(user, ct);
            await eventBus.Publish(user, ct);

            return user;
        }
    }
}
