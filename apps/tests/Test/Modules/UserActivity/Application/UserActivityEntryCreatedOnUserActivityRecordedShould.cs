using AwesomeAssertions;
using Nakama.Shared.Infrastructure.Events;
using Nakama.UserActivity.Application.Handlers;
using Nakama.Users.Domain.Events;
using Test.Modules.Users.Domain;

namespace Test.Modules.UserActivity.Application;

[TestFixture]
public sealed class UserActivityEntryCreatedOnUserActivityRecordedShould : UserActivityUnitTestCase
{
    private UserActivityEntryCreatedOnUserActivityRecorded _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new UserActivityEntryCreatedOnUserActivityRecorded(this.ActivityRepository.Object);
    }

    [Test]
    public async Task save_a_login_entry_for_the_user_at_the_time_of_the_activity()
    {
        UserActivityRecorded @event = UserActivityRecordedMother.Random();

        await this._handler.Handle(new DomainEventNotification<UserActivityRecorded>(@event), CancellationToken.None);

        this.ShouldHaveSavedUserActivityEntryMatching(entry =>
            entry.Id != Guid.Empty
            && entry.UserId == @event.Id.Value
            && entry.Type == "Login"
            && entry.OccurredAt == @event.OccurredAt);
    }

    [Test]
    public async Task save_a_different_entry_for_each_activity_of_the_same_user()
    {
        UserActivityRecorded first = UserActivityRecordedMother.Random();
        UserActivityRecorded second = UserActivityRecordedMother.With(first.Id);

        await this._handler.Handle(new DomainEventNotification<UserActivityRecorded>(first), CancellationToken.None);
        await this._handler.Handle(new DomainEventNotification<UserActivityRecorded>(second), CancellationToken.None);

        this.SavedEntries.Select(entry => entry.Id).Should().HaveCount(2).And.OnlyHaveUniqueItems();
        this.SavedEntries.Select(entry => entry.UserId).Should().OnlyContain(userId => userId == first.Id.Value);
    }
}
