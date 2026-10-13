using System.Linq.Expressions;
using Microsoft.Extensions.Options;
using Moq;
using Nakama.ChatbotSummaries.Domain;
using Nakama.Shared.Infrastructure.Storage;
using Test.Modules.Shared;

namespace Test.Modules.ChatbotSummaries;

public abstract class ChatbotSummariesUnitTestCase : UnitTestCase
{
    protected Mock<IChatbotSummaryRepository> SummariesRepository { get; } = new();

    protected IOptions<StorageOptions> Storage { get; } = Options.Create(StorageOptionsMother.Random());

    [SetUp]
    public void ChatbotSummariesSetUp() => this.SummariesRepository.Reset();

    protected void ShouldFindAllChatbotSummaries(IReadOnlyCollection<ChatbotSummary> summaries) =>
        this.SummariesRepository
            .Setup(repository => repository.FindAll(It.IsAny<CancellationToken>()))
            .ReturnsAsync(summaries);

    protected void ShouldFindChatbotSummary(ChatbotSummary summary) =>
        this.SummariesRepository
            .Setup(repository => repository.FindById(summary.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summary);

    protected void ShouldNotFindChatbotSummary() =>
        this.SummariesRepository
            .Setup(repository => repository.FindById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as ChatbotSummary);

    protected void ShouldHaveSavedChatbotSummaryMatching(Expression<Func<ChatbotSummary, bool>> matches) =>
        this.SummariesRepository.Verify(
            repository => repository.Save(It.Is(matches), It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldHaveUpdatedChatbotSummary(ChatbotSummary summary) =>
        this.SummariesRepository.Verify(
            repository => repository.Update(summary, It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveUpdatedAnyChatbotSummary() =>
        this.SummariesRepository.Verify(
            repository => repository.Update(It.IsAny<ChatbotSummary>(), It.IsAny<CancellationToken>()),
            Times.Never);
}
