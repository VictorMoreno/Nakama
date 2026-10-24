using Moq;
using Nakama.FormationDetail.Application;
using Nakama.FormationDetail.Application.Finder;
using Nakama.FormationEnrollment.Application;
using Nakama.FormationEnrollment.Application.Searcher;
using Nakama.QuestionActivity.Application;
using Nakama.QuestionActivity.Application.Finder;
using Nakama.UserActivity.Application;
using Nakama.UserActivity.Application.Finder;
using Test.Modules.Shared;

namespace Test.Modules.Home;

public abstract class HomeUnitTestCase : UnitTestCase
{
    private const int ActivityWindowDays = 30;

    [SetUp]
    public void HomeSetUp()
    {
        // Method intentionally left empty.
    }

    protected void ShouldFindQuestionsOfTheCurrentWeek(int count) =>
        this.QueryBus
            .Setup(bus => bus.Ask(It.IsAny<QuestionActivityFinder.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new QuestionActivityResponse([], count));

    protected void ShouldFindActiveDays(int activeDays) =>
        this.QueryBus
            .Setup(bus => bus.Ask(It.IsAny<UserActivityFinder.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserActivityResponse(
                Enumerable.Range(0, ActivityWindowDays)
                    .Select(day => new DailyUserActivityRow($"2026-10-{day + 1:00}", day < activeDays ? 1 : 0, day < activeDays ? 1 : 0))
                    .ToList(),
                0,
                0,
                0));

    protected void ShouldFindEnrollmentsOfTheUser(params FormationEnrollmentResponse[] enrollments) =>
        this.QueryBus
            .Setup(bus => bus.Ask(It.IsAny<UserFormationEnrollmentSearcher.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollments);

    protected void ShouldFindFormationDetail(FormationDetailResponse detail) =>
        this.QueryBus
            .Setup(bus => bus.Ask(It.Is<FormationDetailFinder.Query>(query => query.Id == detail.Id), It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);

    protected void ShouldHaveAskedForTheQuestionsOf(Guid userId) =>
        this.QueryBus.Verify(
            bus => bus.Ask(
                It.Is<QuestionActivityFinder.Query>(query => query.Weeks == 1 && query.UserId == userId),
                It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldHaveAskedForTheActivityOf(Guid userId) =>
        this.QueryBus.Verify(
            bus => bus.Ask(
                It.Is<UserActivityFinder.Query>(query => query.Days == ActivityWindowDays && query.UserId == userId),
                It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldHaveAskedForTheEnrollmentsOf(Guid userId) =>
        this.QueryBus.Verify(
            bus => bus.Ask(
                It.Is<UserFormationEnrollmentSearcher.Query>(query => query.UserId == userId),
                It.IsAny<CancellationToken>()),
            Times.Once);
}
