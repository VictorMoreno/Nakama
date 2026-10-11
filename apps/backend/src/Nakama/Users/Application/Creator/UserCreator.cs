using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;
using Nakama.Users.Domain;
using Nakama.Users.Domain.Errors;
using Nakama.Users.Domain.Services;

namespace Nakama.Users.Application.Creator;

public static class UserCreator
{
    public sealed record Command(Guid Id, string Email, string Name, bool IsAdmin, string InitialPassword) : ICommand;

    public sealed class Handler(Creator creator) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await creator.Execute(UserId.From(cmd.Id), new Email(cmd.Email), cmd.Name, cmd.IsAdmin, cmd.InitialPassword, ct);
        }
    }

    public sealed class Creator(
        IUsersRepository repository,
        IEventBus eventBus,
        ISystemClock systemClock) : IService
    {
        private readonly UserEmailAvailabilityChecker _emailAvailabilityChecker = new(repository);

        public async Task Execute(UserId id, Email email, string name, bool isAdmin, string initialPassword, CancellationToken ct)
        {
            await this._emailAvailabilityChecker.EnsureAvailable(email, ct);

            User user = User.Create(id, email, name, isAdmin, systemClock);

            UserCredentialResult result = await repository.CreateWithPassword(user, initialPassword, ct);
            if (!result.Succeeded)
            {
                throw UserErrors.Invalid(string.Join(", ", result.Errors));
            }

            await eventBus.Publish(user, ct);
        }
    }
}
