using System.Linq.Expressions;
using Moq;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Chatbots;

public abstract class ChatbotsUnitTestCase : UnitTestCase
{
    protected Mock<IConversationsRepository> ConversationsRepository { get; } = new();

    protected Mock<IChatbotsRepository> ChatbotsRepository { get; } = new();

    protected Mock<IChatbotIconStorage> IconStorage { get; } = new();

    protected Mock<IChatbotDocumentStorage> DocumentStorage { get; } = new();

    protected Mock<IChatbotDocumentFileReader> DocumentFileReader { get; } = new();

    protected Mock<IConversationAttachmentStorage> AttachmentStorage { get; } = new();

    [SetUp]
    public void ChatbotsSetUp()
    {
        this.ConversationsRepository.Reset();
        this.ChatbotsRepository.Reset();
        this.IconStorage.Reset();
        this.DocumentStorage.Reset();
        this.DocumentFileReader.Reset();
        this.AttachmentStorage.Reset();
    }

    protected void ShouldFindConversation(Conversation? conversation) =>
        this.ConversationsRepository
            .Setup(repository => repository.FindById(It.IsAny<ConversationId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(conversation);

    protected void ShouldFindChatbot(Chatbot? chatbot) =>
        this.ChatbotsRepository
            .Setup(repository => repository.FindById(It.IsAny<ChatbotId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(chatbot);

    protected void ShouldStoreIcon(string fileName) =>
        this.IconStorage
            .Setup(storage => storage.Save(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileName);

    protected void ShouldStoreDocument(string fileName) =>
        this.DocumentStorage
            .Setup(storage => storage.Save(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fileName);

    protected void ShouldFailToStoreDocument(DomainError error) =>
        this.DocumentStorage
            .Setup(storage => storage.Save(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(error);

    protected void ShouldReadDocumentFile(byte[]? content) =>
        this.DocumentFileReader
            .Setup(reader => reader.Read(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(content);

    protected void ShouldHaveRemoved(Conversation conversation) =>
        this.ConversationsRepository.Verify(
            repository => repository.Remove(conversation, It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveRemovedAnyConversation() =>
        this.ConversationsRepository.Verify(
            repository => repository.Remove(It.IsAny<Conversation>(), It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldHaveSavedChatbot(Chatbot chatbot) =>
        this.ChatbotsRepository.Verify(
            repository => repository.Save(chatbot, It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldHaveSavedChatbotMatching(Expression<Func<Chatbot, bool>> matches) =>
        this.ChatbotsRepository.Verify(
            repository => repository.Save(It.Is(matches), It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveSavedAnyChatbot() =>
        this.ChatbotsRepository.Verify(
            repository => repository.Save(It.IsAny<Chatbot>(), It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldHaveStoredIcon(Guid id, string contentType) =>
        this.IconStorage.Verify(
            storage => storage.Save(
                id,
                contentType,
                It.IsAny<Stream>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldHaveDeletedExistingIcon(Guid id) =>
        this.IconStorage.Verify(storage => storage.DeleteExisting(id), Times.Once);

    protected void ShouldNotHaveTouchedTheIconStorage()
    {
        this.IconStorage.Verify(
            storage => storage.Save(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        this.IconStorage.Verify(storage => storage.DeleteExisting(It.IsAny<Guid>()), Times.Never);
    }

    protected void ShouldHaveStoredDocument(Guid documentId, string originalFileName, long length) =>
        this.DocumentStorage.Verify(
            storage => storage.Save(
                documentId,
                originalFileName,
                It.IsAny<Stream>(),
                length,
                It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldNotHaveStoredAnyDocument() =>
        this.DocumentStorage.Verify(
            storage => storage.Save(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Stream>(),
                It.IsAny<long>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

    protected void ShouldHaveDeletedDocumentFile(string fileName) =>
        this.DocumentStorage.Verify(storage => storage.Delete(fileName), Times.Once);

    protected void ShouldNotHaveDeletedAnyDocumentFile() =>
        this.DocumentStorage.Verify(storage => storage.Delete(It.IsAny<string>()), Times.Never);

    protected void ShouldHaveDeletedAttachments(IReadOnlyCollection<string> fileNames) =>
        this.AttachmentStorage.Verify(
            storage => storage.Delete(It.Is<IEnumerable<string>>(names => names.SequenceEqual(fileNames))),
            Times.Once);

    protected void ShouldHavePublishedConversationDeleted(
        ConversationId id,
        IReadOnlyCollection<string> attachmentFileNames) =>
        this.EventBus.Verify(
            bus => bus.Publish(
                It.Is<AggregateRoot>(aggregate => RecordsConversationDeleted(aggregate, id, attachmentFileNames)),
                It.IsAny<CancellationToken>()),
            Times.Once);

    protected void ShouldHavePublishedTeamMemberAdded(ChatbotId id, TeamMember member) =>
        this.EventBus.Verify(
            bus => bus.Publish(
                It.Is<AggregateRoot>(aggregate => RecordsTeamMemberAdded(aggregate, id, member)),
                It.IsAny<CancellationToken>()),
            Times.Once);

    private static bool RecordsConversationDeleted(
        AggregateRoot aggregate,
        ConversationId id,
        IReadOnlyCollection<string> attachmentFileNames) =>
        aggregate.DomainEvents
            .OfType<ConversationDeleted>()
            .Any(@event => @event.Id == id && @event.AttachmentFileNames.SequenceEqual(attachmentFileNames));

    private static bool RecordsTeamMemberAdded(AggregateRoot aggregate, ChatbotId id, TeamMember member) =>
        aggregate.DomainEvents
            .OfType<ChatbotTeamMemberAdded>()
            .Any(@event => @event.Id == id && @event.Member == member);
}
