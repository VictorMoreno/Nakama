using AwesomeAssertions;
using Nakama.Analytics.Application;
using Nakama.Analytics.Application.Finder;
using Nakama.Analytics.Domain.Errors;
using Nakama.QuestionActivity.Application;
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
        this.ShouldFindQuestionsOfTheCurrentWeek(0);
        this.ShouldFindQuestionsByChatbot();
        this.ShouldFindAnswerFeedback(0, 0);
    }

    [Test]
    public async Task return_the_analytics_together_with_the_user_and_question_counts()
    {
        Guid firstBot = Guid.NewGuid();
        Guid secondBot = Guid.NewGuid();
        AnalyticsEntity analytics = AnalyticsMother.Random();
        this.ShouldFindAnalytics(analytics);
        this.ShouldFindActiveUsersOfTheLastWeek(7);
        this.ShouldFindQuestionsOfTheCurrentWeek(12);
        this.ShouldFindQuestionsByChatbot(new ChatbotQuestionsRow(firstBot, 9), new ChatbotQuestionsRow(secondBot, 3));
        this.ShouldFindAnswerFeedback(6, 8);
        this.ShouldCountUsers(20);

        AnalyticsResponse response = await this._handler.Handle(new AnalyticsFinder.Query(), CancellationToken.None);

        response.ActiveBotsCount.Should().Be(analytics.ActiveBotsCount);
        response.UsefulFeedbackCount.Should().Be(6);
        response.TotalFeedbackCount.Should().Be(8);
        response.AnsweredThisWeek.Should().Be(12);
        response.UsageByBot.Should().Equal(new UsageByBotRow(firstBot, 9), new UsageByBotRow(secondBot, 3));
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
    public async Task take_the_questions_answered_this_week_from_the_question_activity_of_the_current_week()
    {
        this.ShouldFindAnalytics(AnalyticsMother.Random());
        this.ShouldFindActiveUsersOfTheLastWeek(3);

        await this._handler.Handle(new AnalyticsFinder.Query(), CancellationToken.None);

        this.ShouldHaveAskedForTheQuestionsOfTheCurrentWeek();
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
