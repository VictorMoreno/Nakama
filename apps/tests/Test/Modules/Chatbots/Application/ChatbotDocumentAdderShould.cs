using AwesomeAssertions;
using Nakama.Chatbots.Application.DocumentAdder;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Nakama.Chatbots.Domain.Events;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.Chatbots.Application;

[TestFixture]
public sealed class ChatbotDocumentAdderShould : ChatbotsUnitTestCase
{
    private ChatbotDocumentAdder.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotDocumentAdder.Handler(new(
            this.ChatbotsRepository.Object,
            this.DocumentStorage.Object,
            this.EventBus.Object,
            this.Clock.Object));
    }

    [Test]
    public async Task store_the_file_and_add_the_document_to_the_chatbot()
    {
        DateTime now = DateTime.UtcNow;
        string fileName = $"{Guid.NewGuid():N}.pdf";
        Chatbot chatbot = ChatbotMother.Random();
        ChatbotDocumentAdder.Command command = ChatbotDocumentAdderCommandMother.With(chatbot.Id.Value);
        this.ShouldBeNow(now);
        this.ShouldFindChatbot(chatbot);
        this.ShouldStoreDocument(fileName);

        await this._handler.Handle(command, CancellationToken.None);

        chatbot.Documents.Should().ContainSingle().Which.Should().Be(new ChatbotDocument(
            command.DocumentId,
            command.Name,
            command.Origin,
            now,
            fileName));
        this.ShouldHaveStoredDocument(command.DocumentId, command.OriginalFileName, command.Length);
        this.ShouldHaveSavedChatbot(chatbot);
        this.ShouldHavePublished<ChatbotDocumentAdded>(
            @event => @event.Id == chatbot.Id && @event.Document.Id == command.DocumentId);
    }

    [Test]
    public async Task fail_without_storing_the_file_when_the_chatbot_does_not_exist()
    {
        this.ShouldNotFindChatbot();

        Func<Task> act = async () =>
            await this._handler.Handle(ChatbotDocumentAdderCommandMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<ChatbotNotFoundError>();
        this.ShouldNotHaveStoredAnyDocument();
        this.ShouldNotHaveSavedAnyChatbot();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_adding_the_document_when_the_file_cannot_be_stored()
    {
        Chatbot chatbot = ChatbotMother.Random();
        this.ShouldFindChatbot(chatbot);
        this.ShouldFailToStoreDocument(ChatbotErrors.DocumentUnsupportedType);

        Func<Task> act = async () => await this._handler.Handle(
            ChatbotDocumentAdderCommandMother.With(chatbot.Id.Value),
            CancellationToken.None);

        await act.Should().ThrowAsync<ChatbotDocumentUnsupportedTypeError>();
        chatbot.Documents.Should().BeEmpty();
        this.ShouldNotHaveSavedAnyChatbot();
        this.ShouldNotHavePublishedAnything();
    }
}
