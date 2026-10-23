using AwesomeAssertions;
using Nakama.Authentication.Domain;
using Nakama.Authentication.Domain.Events;
using Nakama.Shared.Infrastructure.Events;
using Nakama.UserActivity.Application.Handlers;
using Nakama.UserActivity.Domain;

namespace Test.Modules.UserActivity.Application;

[TestFixture]
public sealed class UserActivityEntryCreatedOnSessionRefreshedShould : UserActivityUnitTestCase
{
    private UserActivityEntryCreatedOnSessionRefreshed _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new UserActivityEntryCreatedOnSessionRefreshed(this.ActivityRepository.Object, this.ActivityOptions);
    }

    [Test]
    public async Task save_a_session_entry_when_the_person_has_no_recent_activity()
    {
        SessionRefreshed refreshed = new(RefreshTokenId.New(), Guid.NewGuid(), DateTime.UtcNow);
        this.ShouldCountRecentActivity(0);

        await this._handler.Handle(new DomainEventNotification<SessionRefreshed>(refreshed), CancellationToken.None);

        this.ShouldHaveSavedUserActivityEntryMatching(entry =>
            entry.UserId == refreshed.UserId
            && entry.Type == UserActivityTypes.Session
            && entry.OccurredAt == refreshed.OccurredAt);
    }

    [Test]
    public async Task save_nothing_when_the_person_was_already_active_within_the_interval()
    {
        SessionRefreshed refreshed = new(RefreshTokenId.New(), Guid.NewGuid(), DateTime.UtcNow);
        this.ShouldCountRecentActivity(1);

        await this._handler.Handle(new DomainEventNotification<SessionRefreshed>(refreshed), CancellationToken.None);

        this.ShouldNotHaveSavedAnyUserActivityEntry();
    }

    [Test]
    public async Task look_for_activity_of_that_person_since_the_start_of_the_interval()
    {
        SessionRefreshed refreshed = new(RefreshTokenId.New(), Guid.NewGuid(), DateTime.UtcNow);
        this.ShouldCountRecentActivity(0);

        await this._handler.Handle(new DomainEventNotification<SessionRefreshed>(refreshed), CancellationToken.None);

        UserActivityOfUserSinceSpecification count = this.Counts.Should().ContainSingle().Subject;
        count.UserId.Should().Be(refreshed.UserId);
        count.Since.Should().Be(refreshed.OccurredAt.AddMinutes(-this.ActivityOptions.Value.SessionIntervalMinutes));
    }
}
