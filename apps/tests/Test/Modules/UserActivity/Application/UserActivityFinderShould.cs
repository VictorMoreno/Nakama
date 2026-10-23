using AwesomeAssertions;
using Nakama.UserActivity.Application;
using Nakama.UserActivity.Application.Finder;
using Nakama.UserActivity.Domain;
using Nakama.UserActivity.Domain.Errors;
using Test.Modules.UserActivity.Domain;

namespace Test.Modules.UserActivity.Application;

[TestFixture]
public sealed class UserActivityFinderShould : UserActivityUnitTestCase
{
    private static readonly DateTime Now = new(2026, 10, 22, 10, 0, 0, DateTimeKind.Utc);

    private UserActivityFinder.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new UserActivityFinder.Handler(
            new(this.ActivityRepository.Object, this.Clock.Object, this.ActivityOptions));
        this.ShouldBeNow(Now);
        this.ShouldFindActivity([]);
    }

    [Test]
    public async Task return_a_row_per_requested_day_from_the_oldest_to_today_even_when_there_was_no_activity()
    {
        UserActivityResponse response = await this._handler.Handle(new UserActivityFinder.Query(3), CancellationToken.None);

        response.Days.Select(day => day.Date).Should().Equal("2026-10-20", "2026-10-21", "2026-10-22");
        response.Days.Should().OnlyContain(day => day.Logins == 0 && day.ActiveUsers == 0);
        response.ActiveToday.Should().Be(0);
    }

    [Test]
    public async Task count_the_logins_and_the_different_people_active_each_day()
    {
        Guid ana = Guid.NewGuid();
        Guid bruno = Guid.NewGuid();
        this.ShouldFindActivity(
        [
            UserActivityEntryMother.With(ana, UserActivityTypes.Login, new DateTime(2026, 10, 21, 8, 0, 0, DateTimeKind.Utc)),
            UserActivityEntryMother.With(ana, UserActivityTypes.Session, new DateTime(2026, 10, 21, 12, 0, 0, DateTimeKind.Utc)),
            UserActivityEntryMother.With(bruno, UserActivityTypes.Login, new DateTime(2026, 10, 21, 9, 0, 0, DateTimeKind.Utc)),
            UserActivityEntryMother.With(ana, UserActivityTypes.Login, new DateTime(2026, 10, 22, 8, 0, 0, DateTimeKind.Utc))
        ]);

        UserActivityResponse response = await this._handler.Handle(new UserActivityFinder.Query(2), CancellationToken.None);

        response.Days.Should().Equal(
            new DailyUserActivityRow("2026-10-21", 2, 2),
            new DailyUserActivityRow("2026-10-22", 1, 1));
        response.ActiveToday.Should().Be(1);
    }

    [Test]
    public async Task bucket_the_days_in_the_configured_time_zone_and_not_in_utc()
    {
        this.ShouldFindActivity(
        [
            UserActivityEntryMother.With(null, UserActivityTypes.Login, new DateTime(2026, 10, 21, 22, 30, 0, DateTimeKind.Utc))
        ]);

        UserActivityResponse response = await this._handler.Handle(new UserActivityFinder.Query(2), CancellationToken.None);

        response.Days.Should().Equal(
            new DailyUserActivityRow("2026-10-21", 0, 0),
            new DailyUserActivityRow("2026-10-22", 1, 1));
    }

    [Test]
    public async Task keep_the_days_right_across_the_change_to_summer_time()
    {
        this.ShouldBeNow(new DateTime(2026, 3, 30, 12, 0, 0, DateTimeKind.Utc));
        this.ShouldFindActivity(
        [
            UserActivityEntryMother.With(null, UserActivityTypes.Login, new DateTime(2026, 3, 28, 23, 30, 0, DateTimeKind.Utc)),
            UserActivityEntryMother.With(null, UserActivityTypes.Login, new DateTime(2026, 3, 29, 21, 30, 0, DateTimeKind.Utc)),
            UserActivityEntryMother.With(null, UserActivityTypes.Login, new DateTime(2026, 3, 29, 22, 30, 0, DateTimeKind.Utc))
        ]);

        UserActivityResponse response = await this._handler.Handle(new UserActivityFinder.Query(3), CancellationToken.None);

        response.Days.Select(day => (day.Date, day.Logins)).Should().Equal(
            ("2026-03-28", 0),
            ("2026-03-29", 2),
            ("2026-03-30", 1));
    }

    [Test]
    public async Task count_the_people_active_today_in_the_last_seven_days_and_in_the_last_thirty()
    {
        this.ShouldFindActivity(
        [
            UserActivityEntryMother.With(null, UserActivityTypes.Login, Now.AddHours(-1)),
            UserActivityEntryMother.With(null, UserActivityTypes.Login, Now.AddDays(-6)),
            UserActivityEntryMother.With(null, UserActivityTypes.Login, Now.AddDays(-7)),
            UserActivityEntryMother.With(null, UserActivityTypes.Login, Now.AddDays(-29)),
            UserActivityEntryMother.With(null, UserActivityTypes.Login, Now.AddDays(-31))
        ]);

        UserActivityResponse response = await this._handler.Handle(new UserActivityFinder.Query(7), CancellationToken.None);

        response.ActiveToday.Should().Be(1);
        response.ActiveLast7Days.Should().Be(2);
        response.ActiveLast30Days.Should().Be(4);
    }

    [Test]
    public async Task count_a_person_once_however_many_times_they_were_active()
    {
        Guid ana = Guid.NewGuid();
        this.ShouldFindActivity(
        [
            UserActivityEntryMother.With(ana, UserActivityTypes.Login, Now.AddHours(-1)),
            UserActivityEntryMother.With(ana, UserActivityTypes.Session, Now.AddHours(-2)),
            UserActivityEntryMother.With(ana, UserActivityTypes.Login, Now.AddDays(-3))
        ]);

        UserActivityResponse response = await this._handler.Handle(new UserActivityFinder.Query(7), CancellationToken.None);

        response.ActiveToday.Should().Be(1);
        response.ActiveLast7Days.Should().Be(1);
        response.ActiveLast30Days.Should().Be(1);
    }

    [Test]
    public async Task look_back_thirty_days_for_the_summary_even_when_fewer_days_are_requested()
    {
        await this._handler.Handle(new UserActivityFinder.Query(7), CancellationToken.None);

        UserActivityBetweenSpecification search = this.Searches.Should().ContainSingle().Subject;
        search.FromInclusive.Should().Be(new DateTime(2026, 9, 22, 22, 0, 0, DateTimeKind.Utc));
        search.ToExclusive.Should().Be(new DateTime(2026, 10, 22, 22, 0, 0, DateTimeKind.Utc));
    }

    [Test]
    public async Task use_the_default_number_of_days_when_none_is_requested()
    {
        UserActivityResponse response = await this._handler.Handle(new UserActivityFinder.Query(null), CancellationToken.None);

        response.Days.Should().HaveCount(this.ActivityOptions.Value.DefaultDays);
    }

    [Test]
    public async Task look_back_as_far_as_requested_when_more_than_thirty_days_are_asked_for()
    {
        await this._handler.Handle(new UserActivityFinder.Query(90), CancellationToken.None);

        this.Searches.Should().ContainSingle().Which.FromInclusive
            .Should().Be(new DateTime(2026, 7, 24, 22, 0, 0, DateTimeKind.Utc));
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(91)]
    public async Task reject_a_number_of_days_out_of_range(int days)
    {
        Func<Task> act = async () => await this._handler.Handle(new UserActivityFinder.Query(days), CancellationToken.None);

        await act.Should().ThrowAsync<UserActivityInvalidDaysError>();
        this.Searches.Should().BeEmpty();
    }
}
