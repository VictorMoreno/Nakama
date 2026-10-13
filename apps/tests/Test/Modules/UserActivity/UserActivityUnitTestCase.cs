using System.Linq.Expressions;
using Moq;
using Nakama.UserActivity.Domain;
using Test.Modules.Shared;

namespace Test.Modules.UserActivity;

public abstract class UserActivityUnitTestCase : UnitTestCase
{
    private readonly List<UserActivityEntry> _savedEntries = [];

    protected Mock<IUserActivityRepository> ActivityRepository { get; } = new();

    protected IReadOnlyList<UserActivityEntry> SavedEntries => this._savedEntries;

    [SetUp]
    public void UserActivitySetUp()
    {
        this.ActivityRepository.Reset();
        this._savedEntries.Clear();
        this.ActivityRepository
            .Setup(repository => repository.Save(It.IsAny<UserActivityEntry>(), It.IsAny<CancellationToken>()))
            .Callback<UserActivityEntry, CancellationToken>((entry, _) => this._savedEntries.Add(entry))
            .Returns(Task.CompletedTask);
    }

    protected void ShouldHaveSavedUserActivityEntryMatching(Expression<Func<UserActivityEntry, bool>> matches) =>
        this.ActivityRepository.Verify(
            repository => repository.Save(It.Is(matches), It.IsAny<CancellationToken>()),
            Times.Once);
}
