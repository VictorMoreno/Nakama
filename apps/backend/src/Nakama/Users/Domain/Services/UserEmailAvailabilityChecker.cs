using Nakama.Users.Domain.Errors;

namespace Nakama.Users.Domain.Services;

public sealed class UserEmailAvailabilityChecker(IUsersRepository repository)
{
    public async Task EnsureAvailable(Email email, CancellationToken cancellationToken)
    {
        if (await repository.GetByEmail(email, cancellationToken) is not null)
        {
            throw UserErrors.EmailAlreadyInUse(email.Value);
        }
    }
}
