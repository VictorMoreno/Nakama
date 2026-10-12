using System.Linq.Expressions;
using Moq;
using Nakama.Analytics.Domain;
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

    protected void ShouldCountActiveUsers(int count) =>
        this.UsersRepository
            .Setup(repository => repository.CountActiveSince(It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(count);

    protected void ShouldCountUsers(int count) =>
        this.UsersRepository
            .Setup(repository => repository.CountAll(It.IsAny<CancellationToken>()))
            .ReturnsAsync(count);

    protected void ShouldHaveCountedActiveUsersSince(DateTime cutoff) =>
        this.UsersRepository.Verify(
            repository => repository.CountActiveSince(cutoff, It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldHaveSavedAnalyticsMatching(Expression<Func<AnalyticsEntity, bool>> matches) =>
        this.AnalyticsRepository.Verify(
            repository => repository.Save(It.Is<AnalyticsEntity>(matches), It.IsAny<CancellationToken>()),
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
