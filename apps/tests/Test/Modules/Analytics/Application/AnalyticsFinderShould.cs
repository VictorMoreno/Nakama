using AwesomeAssertions;
using Nakama.Analytics.Application;
using Nakama.Analytics.Application.Finder;
using Nakama.Analytics.Domain.Errors;
using Test.Modules.Analytics.Domain;
using AnalyticsEntity = Nakama.Analytics.Domain.Analytics;

namespace Test.Modules.Analytics.Application;

[TestFixture]
public sealed class AnalyticsFinderShould : AnalyticsUnitTestCase
{
    private AnalyticsFinder.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new AnalyticsFinder.Handler(
            new(this.AnalyticsRepository.Object, this.UsersRepository.Object, this.Clock.Object));
    }

    [Test]
    public async Task return_the_analytics_together_with_the_user_counts()
    {
        AnalyticsEntity analytics = AnalyticsMother.WithUsageRow();
        this.ShouldBeNow(DateTime.UtcNow);
        this.ShouldFindAnalytics(analytics);
        this.ShouldCountActiveUsers(7);
        this.ShouldCountUsers(20);

        AnalyticsResponse response = await this._handler.Handle(new AnalyticsFinder.Query(), CancellationToken.None);

        response.Should().BeEquivalentTo(analytics);
        response.ActiveUsersCount.Should().Be(7);
        response.TotalUsersCount.Should().Be(20);
    }

    [Test]
    public async Task count_the_active_users_of_the_last_seven_days()
    {
        DateTime now = DateTime.UtcNow;
        this.ShouldBeNow(now);
        this.ShouldFindAnalytics(AnalyticsMother.Random());

        await this._handler.Handle(new AnalyticsFinder.Query(), CancellationToken.None);

        this.ShouldHaveCountedActiveUsersSince(now.AddDays(-7));
    }

    [Test]
    public async Task fail_when_there_are_no_analytics_yet()
    {
        this.ShouldBeNow(DateTime.UtcNow);
        this.ShouldFindAnalytics(null);

        Func<Task> act = async () =>
            await this._handler.Handle(new AnalyticsFinder.Query(), CancellationToken.None);

        await act.Should().ThrowAsync<AnalyticsNotFoundError>();
    }
}
