using Nakama.Shared.Infrastructure.Events;
using Nakama.UserActivity.Domain;
using Nakama.Users.Domain.Events;

namespace Nakama.UserActivity.Application.Handlers;

// "Login" is the only Type value today because UserActivityRecorded only ever fires from
// User.MarkActive, which only ever fires after a successful login (no logout flow exists yet -
// there is no session/cookie to log out of, see ADR entry). Extend the event with a real
// activity-kind enum when a logout flow exists, instead of guessing at one now.
public sealed class UserActivityEntryCreatedOnUserActivityRecorded(IUserActivityRepository repository)
    : DomainEventHandler<UserActivityRecorded>
{
    protected override Task Handle(UserActivityRecorded userActivityRecorded, CancellationToken ct) =>
        repository.Save(
            UserActivityEntry.Create(Guid.NewGuid(), userActivityRecorded.Id.Value, "Login", userActivityRecorded.OccurredAt),
            ct);
}
