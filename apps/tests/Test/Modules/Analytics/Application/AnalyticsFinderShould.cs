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
            new(this.AnalyticsRepository.Object, this.UsersRepository.Object, this.QueryBus.Object));
    }

    [Test]
    public async Task return_the_analytics_together_with_the_user_counts()
    {
        AnalyticsEntity analytics = AnalyticsMother.WithUsageRow();
        this.ShouldFindAnalytics(analytics);
        this.ShouldFindActiveUsersOfTheLastWeek(7);
        this.ShouldCountUsers(20);

        AnalyticsResponse response = await this._handler.Handle(new AnalyticsFinder.Query(), CancellationToken.None);

        response.Should().BeEquivalentTo(analytics);
        response.ActiveUsersCount.Should().Be(7);
        response.TotalUsersCount.Should().Be(20);
    }

    [Test]
    public async Task take_the_active_users_from_the_user_activity_of_the_last_seven_days()
    {
        this.ShouldFindAnalytics(AnalyticsMother.Random());
        this.ShouldFindActiveUsersOfTheLastWeek(3);

        await this._handler.Handle(new AnalyticsFinder.Query(), CancellationToken.None);

        this.ShouldHaveAskedForTheActivityOfTheLastDays(7);
    }

    [Test]
    public async Task fail_when_there_are_no_analytics_yet()
    {
        this.ShouldFindAnalytics(null);

        Func<Task> act = async () =>
            await this._handler.Handle(new AnalyticsFinder.Query(), CancellationToken.None);

        await act.Should().ThrowAsync<AnalyticsNotFoundError>();
    }
}
