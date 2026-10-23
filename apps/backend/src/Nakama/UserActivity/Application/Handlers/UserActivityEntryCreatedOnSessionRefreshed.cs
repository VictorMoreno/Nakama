using Microsoft.Extensions.Options;
using Nakama.Authentication.Domain.Events;
using Nakama.Shared.Infrastructure.Events;
using Nakama.Shared.Infrastructure.UserActivity;
using Nakama.UserActivity.Domain;

namespace Nakama.UserActivity.Application.Handlers;

public sealed class UserActivityEntryCreatedOnSessionRefreshed(
    IUserActivityRepository repository,
    IOptions<UserActivityOptions> options) : DomainEventHandler<SessionRefreshed>
{
    protected override async Task Handle(SessionRefreshed sessionRefreshed, CancellationToken cancellationToken)
    {
        DateTime since = sessionRefreshed.OccurredAt.AddMinutes(-options.Value.SessionIntervalMinutes);
        int recent = await repository.Count(
            new UserActivityOfUserSinceSpecification(sessionRefreshed.UserId, since),
            cancellationToken);

        if (recent > 0)
        {
            return;
        }

        await repository.Save(
            UserActivityEntry.Create(
                Guid.NewGuid(),
                sessionRefreshed.UserId,
                UserActivityTypes.Session,
                sessionRefreshed.OccurredAt),
            cancellationToken);
    }
}
