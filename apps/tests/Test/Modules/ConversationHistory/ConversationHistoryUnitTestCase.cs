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
}
