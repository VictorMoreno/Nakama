using System.Linq.Expressions;
using Moq;
using Nakama.Analytics.Domain;
using Nakama.Shared.Domain.Specification;
using Nakama.UserActivity.Application;
using Nakama.UserActivity.Application.Finder;
using Nakama.Users.Domain;
using Test.Modules.Shared;
using AnalyticsEntity = Nakama.Analytics.Domain.Analytics;

namespace Test.Modules.Analytics;

public abstract class AnalyticsUnitTestCase : UnitTestCase
{
    protected Mock<IAnalyticsRepository> AnalyticsRepository { get; } = new();

    protected Mock<IUsersRepository> UsersRepository { get; } = new();

    [SetUp]
    public void AnalyticsSetUp()
    {
        this.AnalyticsRepository.Reset();
        this.UsersRepository.Reset();
    }

    protected void ShouldFindAnalytics(AnalyticsEntity? analytics) =>
        this.AnalyticsRepository
            .Setup(repository => repository.Find(It.IsAny<CancellationToken>()))
            .ReturnsAsync(analytics);

    protected void ShouldFindActiveUsersOfTheLastWeek(int count) =>
        this.QueryBus
            .Setup(bus => bus.Ask(It.IsAny<UserActivityFinder.Query>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserActivityResponse([], 0, count, count));

    protected void ShouldHaveAskedForTheActivityOfTheLastDays(int days) =>
        this.QueryBus.Verify(
            bus => bus.Ask(
                It.Is<UserActivityFinder.Query>(query => query.Days == days),
                It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldCountUsers(int count) =>
        this.UsersRepository
            .Setup(repository => repository.Count(
                It.Is<ISpecification<User>>(specification => specification is UserListingSpecification),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(count);

    protected void ShouldHaveSavedAnalyticsMatching(Expression<Func<AnalyticsEntity, bool>> matches) =>
        this.AnalyticsRepository.Verify(
            repository => repository.Save(It.Is(matches), It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveSavedAnyAnalytics() =>
        this.AnalyticsRepository.Verify(
            repository => repository.Save(It.IsAny<AnalyticsEntity>(), It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldHaveUpdatedAnalytics(AnalyticsEntity analytics) =>
        this.AnalyticsRepository.Verify(
            repository => repository.Update(analytics, It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveUpdatedAnyAnalytics() =>
        this.AnalyticsRepository.Verify(
            repository => repository.Update(It.IsAny<AnalyticsEntity>(), It.IsAny<CancellationToken>()),
            Times.Never);
}
