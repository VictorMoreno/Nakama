using Moq;
using Nakama.AnswerFeedback.Application;
using Nakama.AnswerFeedback.Application.Finder;
using Nakama.QuestionActivity.Application;
using Nakama.QuestionActivity.Application.Finder;
using Nakama.UserActivity.Application;
using Nakama.UserActivity.Application.Finder;
using Nakama.Users.Application;
using Nakama.Users.Application.CountFinder;
using Test.Modules.Shared;

namespace Test.Modules.Analytics;

public abstract class AnalyticsUnitTestCase : UnitTestCase
{
    [SetUp]
    public void AnalyticsSetUp()
    {
    }

    protected void ShouldFindActiveUsersOfTheLastWeek(int count) =>
        this.QueryBus
            .Setup(bus => bus.Ask(It.IsAny<UserActivityFinder.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserActivityResponse([], 0, count, count));

    protected void ShouldFindQuestionsOfTheCurrentWeek(int count) =>
        this.QueryBus
            .Setup(bus => bus.Ask(It.IsAny<QuestionActivityFinder.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuestionActivityResponse([], count));

    protected void ShouldFindAnswerFeedback(int useful, int total) =>
        this.QueryBus
            .Setup(bus => bus.Ask(It.IsAny<AnswerFeedbackSummaryFinder.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AnswerFeedbackSummaryResponse(useful, total));

    protected void ShouldFindQuestionsByChatbot(params ChatbotQuestionsRow[] rows) =>
        this.QueryBus
            .Setup(bus => bus.Ask(It.IsAny<QuestionsByChatbotFinder.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuestionsByChatbotResponse(rows));

    protected void ShouldHaveAskedForTheQuestionsOfTheCurrentWeek() =>
        this.QueryBus.Verify(
            bus => bus.Ask(
                It.Is<QuestionActivityFinder.Query>(query => query.Weeks == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldHaveAskedForTheActivityOfTheLastDays(int days) =>
        this.QueryBus.Verify(
            bus => bus.Ask(
                It.Is<UserActivityFinder.Query>(query => query.Days == days),
                It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldCountUsers(int count) =>
        this.QueryBus
            .Setup(bus => bus.Ask(It.IsAny<UsersCountFinder.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UsersCountResponse(count));
}
