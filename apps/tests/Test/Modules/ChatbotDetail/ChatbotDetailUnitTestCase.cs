using Microsoft.Extensions.Options;
using Moq;
using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Storage;
using Test.Modules.Shared;
using ChatbotDetailEntity = Nakama.ChatbotDetail.Domain.ChatbotDetail;

namespace Test.Modules.ChatbotDetail;

public abstract class ChatbotDetailUnitTestCase : UnitTestCase
{
    protected Mock<IChatbotDetailRepository> DetailRepository { get; } = new();

    protected IOptions<StorageOptions> Storage { get; } = Options.Create(StorageOptionsMother.Random());

    [SetUp]
    public void ChatbotDetailSetUp() => this.DetailRepository.Reset();

    protected void ShouldFindChatbotDetail(ChatbotDetailEntity? detail) =>
        this.DetailRepository
            .Setup(repository => repository.FindById(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(detail);
}
