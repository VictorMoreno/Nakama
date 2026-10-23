using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using Moq;
using Nakama.Shared.Domain.Specification;
using Nakama.Shared.Infrastructure.UserActivity;
using Nakama.UserActivity.Domain;
using Test.Modules.Shared;

namespace Test.Modules.UserActivity;

public abstract class UserActivityUnitTestCase : UnitTestCase
{
    private readonly List<UserActivityEntry> _savedEntries = [];

    private readonly List<UserActivityBetweenSpecification> _searches = [];

    private readonly List<UserActivityOfUserSinceSpecification> _counts = [];

    protected Mock<IUserActivityRepository> ActivityRepository { get; } = new();

    protected IOptions<UserActivityOptions> ActivityOptions { get; } = Options.Create(UserActivityOptionsMother.Random());

    protected IReadOnlyList<UserActivityEntry> SavedEntries => this._savedEntries;

    protected IReadOnlyList<UserActivityBetweenSpecification> Searches => this._searches;

    protected IReadOnlyList<UserActivityOfUserSinceSpecification> Counts => this._counts;

    [SetUp]
    public void UserActivitySetUp()
    {
        this.ActivityRepository.Reset();
        this._savedEntries.Clear();
        this._searches.Clear();
        this._counts.Clear();
        this.ActivityRepository
            .Setup(repository => repository.Save(It.IsAny<UserActivityEntry>(), It.IsAny<CancellationToken>()))
            .Callback<UserActivityEntry, CancellationToken>((entry, _) => this._savedEntries.Add(entry))
            .Returns(Task.CompletedTask);
    }

    protected void ShouldFindActivity(IReadOnlyList<UserActivityEntry> entries) =>
        this.ActivityRepository
            .Setup(repository => repository.Search(
                It.IsAny<ISpecification<UserActivityEntry>>(),
                It.IsAny<CancellationToken>()))
            .Callback<ISpecification<UserActivityEntry>, CancellationToken>((specification, _) =>
                this._searches.Add((UserActivityBetweenSpecification)specification))
            .ReturnsAsync(entries);

    protected void ShouldCountRecentActivity(int count) =>
        this.ActivityRepository
            .Setup(repository => repository.Count(
                It.IsAny<ISpecification<UserActivityEntry>>(),
                It.IsAny<CancellationToken>()))
            .Callback<ISpecification<UserActivityEntry>, CancellationToken>((specification, _) =>
                this._counts.Add((UserActivityOfUserSinceSpecification)specification))
            .ReturnsAsync(count);

    protected void ShouldHaveSavedUserActivityEntryMatching(Expression<Func<UserActivityEntry, bool>> matches) =>
        this.ActivityRepository.Verify(
            repository => repository.Save(It.Is(matches), It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveSavedAnyUserActivityEntry() =>
        this.ActivityRepository.Verify(
            repository => repository.Save(It.IsAny<UserActivityEntry>(), It.IsAny<CancellationToken>()),
            Times.Never);
}
