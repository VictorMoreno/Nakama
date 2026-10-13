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
}
