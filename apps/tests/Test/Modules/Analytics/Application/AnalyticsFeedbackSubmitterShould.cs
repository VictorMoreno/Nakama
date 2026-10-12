using AwesomeAssertions;
using Nakama.Analytics.Application.FeedbackSubmitter;
using Test.Modules.Analytics.Domain;
using AnalyticsEntity = Nakama.Analytics.Domain.Analytics;

namespace Test.Modules.Analytics.Application;

[TestFixture]
public sealed class AnalyticsFeedbackSubmitterShould : AnalyticsUnitTestCase
{
    private AnalyticsFeedbackSubmitter.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new AnalyticsFeedbackSubmitter.Handler(new(this.AnalyticsRepository.Object));
    }

    [Test]
    public async Task create_the_analytics_counting_a_useful_feedback_when_none_exist()
    {
        this.ShouldFindAnalytics(null);

        await this._handler.Handle(AnalyticsFeedbackSubmitterCommandMother.With(true), CancellationToken.None);

        this.ShouldHaveSavedAnalyticsMatching(analytics =>
            analytics.TotalFeedbackCount == 1 && analytics.UsefulFeedbackCount == 1);
        this.ShouldNotHaveUpdatedAnyAnalytics();
    }

    [Test]
    public async Task create_the_analytics_counting_a_not_useful_feedback_when_none_exist()
    {
        this.ShouldFindAnalytics(null);

        await this._handler.Handle(AnalyticsFeedbackSubmitterCommandMother.With(false), CancellationToken.None);

        this.ShouldHaveSavedAnalyticsMatching(analytics =>
            analytics.TotalFeedbackCount == 1 && analytics.UsefulFeedbackCount == 0);
    }

    [Test]
    public async Task add_a_useful_feedback_to_the_existing_counters()
    {
        AnalyticsEntity analytics = AnalyticsMother.With(totalFeedbackCount: 5, usefulFeedbackCount: 3);
        this.ShouldFindAnalytics(analytics);

        await this._handler.Handle(AnalyticsFeedbackSubmitterCommandMother.With(true), CancellationToken.None);

        analytics.TotalFeedbackCount.Should().Be(6);
        analytics.UsefulFeedbackCount.Should().Be(4);
        this.ShouldHaveUpdatedAnalytics(analytics);
        this.ShouldNotHaveSavedAnyAnalytics();
    }

    [Test]
    public async Task add_a_not_useful_feedback_without_touching_the_useful_counter()
    {
        AnalyticsEntity analytics = AnalyticsMother.With(totalFeedbackCount: 5, usefulFeedbackCount: 3);
        this.ShouldFindAnalytics(analytics);

        await this._handler.Handle(AnalyticsFeedbackSubmitterCommandMother.With(false), CancellationToken.None);

        analytics.TotalFeedbackCount.Should().Be(6);
        analytics.UsefulFeedbackCount.Should().Be(3);
        this.ShouldHaveUpdatedAnalytics(analytics);
    }
}
