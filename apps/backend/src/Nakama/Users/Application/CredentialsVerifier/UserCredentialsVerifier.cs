using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;
using Nakama.Users.Domain;
using Nakama.Users.Domain.Errors;

namespace Nakama.Users.Application.CredentialsVerifier;

public static class UserCredentialsVerifier
{
    public sealed record Query(string Email, string Password) : IQuery<UserResponse>;

    public sealed class Handler(Verifier verifier) : IQueryHandler<Query, UserResponse>
    {
        public async Task<UserResponse> Handle(Query query, CancellationToken ct)
        {
            User user = await verifier.Execute(new Email(query.Email), query.Password, ct);
            return (UserResponse)user;
        }
    }

    public sealed class Verifier(IUsersRepository repository, IEventBus eventBus, ISystemClock systemClock) : IService
    {
        public async Task<User> Execute(Email email, string password, CancellationToken ct)
        {
            IReadOnlyList<User> matching = await repository.Search(new UserByEmailSpecification(email), ct);
            User? user = matching.FirstOrDefault();

            if (user is null || !await repository.VerifyPassword(user, password))
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
