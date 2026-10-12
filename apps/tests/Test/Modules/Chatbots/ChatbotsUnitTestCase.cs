using Moq;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Chatbots;

public abstract class ChatbotsUnitTestCase : UnitTestCase
{
    protected Mock<IConversationsRepository> ConversationsRepository { get; } = new();

    [SetUp]
    public void ChatbotsSetUp() => this.ConversationsRepository.Reset();

    protected void ShouldFindConversation(Conversation? conversation) =>
        this.ConversationsRepository
            .Setup(repository => repository.FindById(It.IsAny<ConversationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

    protected void ShouldHaveRemoved(Conversation conversation) =>
        this.ConversationsRepository.Verify(
            repository => repository.Remove(conversation, It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveRemovedAnyConversation() =>
        this.ConversationsRepository.Verify(
            repository => repository.Remove(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldHavePublishedConversationDeleted(
        ConversationId id,
        IReadOnlyCollection<string> attachmentFileNames) =>
        this.EventBus.Verify(
            bus => bus.Publish(
                It.Is<AggregateRoot>(aggregate => RecordsConversationDeleted(aggregate, id, attachmentFileNames)),
                It.IsAny<CancellationToken>()),
            Times.Once);

    private static bool RecordsConversationDeleted(
        AggregateRoot aggregate,
        ConversationId id,
        IReadOnlyCollection<string> attachmentFileNames) =>
        aggregate.DomainEvents
            .OfType<ConversationDeleted>()
            .Any(@event => @event.Id == id && @event.AttachmentFileNames.SequenceEqual(attachmentFileNames));
}
