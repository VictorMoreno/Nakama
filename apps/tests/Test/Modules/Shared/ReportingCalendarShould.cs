using AwesomeAssertions;
using Microsoft.Extensions.Options;
using Nakama.Shared.Infrastructure.Reporting;

namespace Test.Modules.Shared;

[TestFixture]
public sealed class ReportingCalendarShould : UnitTestCase
{
    private ReportingCalendar Create(string? timeZone = null) =>
        new(this.Clock.Object, Options.Create(ReportingOptionsMother.With(timeZone)));

    [Test]
    public void take_today_in_the_configured_time_zone_and_not_in_utc()
    {
        this.ShouldBeNow(new DateTime(2026, 10, 21, 22, 30, 0, DateTimeKind.Utc));

        this.Create().Today().Should().Be(new DateOnly(2026, 10, 22));
        this.Create("UTC").Today().Should().Be(new DateOnly(2026, 10, 21));
    }

    [Test]
    public void convert_a_utc_moment_to_the_local_day_whatever_its_kind()
    {
        ReportingCalendar calendar = this.Create();

        calendar.LocalDay(new DateTime(2026, 10, 21, 22, 30, 0, DateTimeKind.Utc)).Should().Be(new DateOnly(2026, 10, 22));
        calendar.LocalDay(new DateTime(2026, 10, 21, 21, 30, 0, DateTimeKind.Unspecified)).Should().Be(new DateOnly(2026, 10, 21));
    }

    [Test]
    public void start_a_local_day_at_the_utc_instant_of_its_local_midnight()
    {
        ReportingCalendar calendar = this.Create();

        calendar.StartOfDayUtc(new DateOnly(2026, 10, 22)).Should().Be(new DateTime(2026, 10, 21, 22, 0, 0, DateTimeKind.Utc));
        calendar.StartOfDayUtc(new DateOnly(2026, 1, 15)).Should().Be(new DateTime(2026, 1, 14, 23, 0, 0, DateTimeKind.Utc));
    }

    [Test]
    public void keep_the_local_midnights_right_across_the_change_to_summer_time()
    {
        ReportingCalendar calendar = this.Create();

        calendar.StartOfDayUtc(new DateOnly(2026, 3, 29)).Should().Be(new DateTime(2026, 3, 28, 23, 0, 0, DateTimeKind.Utc));
        calendar.StartOfDayUtc(new DateOnly(2026, 3, 30)).Should().Be(new DateTime(2026, 3, 29, 22, 0, 0, DateTimeKind.Utc));
    }

    [TestCase(2026, 10, 19, 2026, 10, 19)]
    [TestCase(2026, 10, 22, 2026, 10, 19)]
    [TestCase(2026, 10, 25, 2026, 10, 19)]
    [TestCase(2026, 10, 26, 2026, 10, 26)]
    public void start_every_week_on_monday(int year, int month, int day, int mondayYear, int mondayMonth, int mondayDay)
    {
        this.Create().WeekStart(new DateOnly(year, month, day)).Should().Be(new DateOnly(mondayYear, mondayMonth, mondayDay));
    }
}
