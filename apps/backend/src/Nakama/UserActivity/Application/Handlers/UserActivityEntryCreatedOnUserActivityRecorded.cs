using Nakama.Shared.Infrastructure.Events;
using Nakama.UserActivity.Domain;
using Nakama.Users.Domain.Events;

namespace Nakama.UserActivity.Application.Handlers;

public sealed class UserActivityEntryCreatedOnUserActivityRecorded(IUserActivityRepository repository)
    : DomainEventHandler<UserActivityRecorded>
{
    protected override Task Handle(UserActivityRecorded userActivityRecorded, CancellationToken cancellationToken) =>
        repository.Save(
            UserActivityEntry.Create(
                Guid.NewGuid(),
                userActivityRecorded.Id.Value,
                UserActivityTypes.Login,
                userActivityRecorded.OccurredAt),
            cancellationToken);
}
