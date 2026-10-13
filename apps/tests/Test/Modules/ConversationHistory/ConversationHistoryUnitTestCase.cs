using System.Linq.Expressions;
using Moq;
using Nakama.ConversationHistory.Domain;
using Test.Modules.Shared;
using ConversationHistoryEntity = Nakama.ConversationHistory.Domain.ConversationHistory;

namespace Test.Modules.ConversationHistory;

public abstract class ConversationHistoryUnitTestCase : UnitTestCase
{
    protected Mock<IConversationHistoryRepository> HistoriesRepository { get; } = new();

    [SetUp]
    public void ConversationHistorySetUp() => this.HistoriesRepository.Reset();

    protected void ShouldFindConversationHistory(ConversationHistoryEntity history) =>
        this.HistoriesRepository
            .Setup(repository => repository.FindById(history.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

    protected void ShouldNotFindConversationHistory() =>
        this.HistoriesRepository
            .Setup(repository => repository.FindById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as ConversationHistoryEntity);

    protected void ShouldHaveSavedConversationHistoryMatching(Expression<Func<ConversationHistoryEntity, bool>> matches) =>
        this.HistoriesRepository.Verify(
            repository => repository.Save(It.Is(matches), It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldHaveUpdatedConversationHistory(ConversationHistoryEntity history) =>
        this.HistoriesRepository.Verify(
            repository => repository.Update(history, It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveUpdatedAnyConversationHistory() =>
        this.HistoriesRepository.Verify(
            repository => repository.Update(It.IsAny<ConversationHistoryEntity>(), It.IsAny<CancellationToken>()),
            Times.Never);
}
