using AwesomeAssertions;
using Nakama.Chatbots.Application.DocumentRemover;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Nakama.Chatbots.Domain.Events;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.Chatbots.Application;

[TestFixture]
public sealed class ChatbotDocumentRemoverShould : ChatbotsUnitTestCase
{
    private ChatbotDocumentRemover.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotDocumentRemover.Handler(
            new(this.ChatbotsRepository.Object, this.DocumentStorage.Object, this.EventBus.Object));
    }

    [Test]
    public async Task remove_the_document_at_the_index_and_delete_its_file()
    {
        ChatbotDocument first = ChatbotDocumentMother.Random();
        ChatbotDocument second = ChatbotDocumentMother.Random();
        Chatbot chatbot = ChatbotMother.With(documents: [first, second]);
        this.ShouldFindChatbot(chatbot);

        await this._handler.Handle(ChatbotDocumentRemoverCommandMother.With(chatbot.Id.Value, 1), CancellationToken.None);

        chatbot.Documents.Should().ContainSingle().Which.Should().Be(first);
        this.ShouldHaveDeletedDocumentFile(second.FileName);
        this.ShouldHaveSavedChatbot(chatbot);
        this.ShouldHavePublished<ChatbotDocumentRemoved>(@event => @event.Id == chatbot.Id && @event.Index == 1);
    }

    [Test]
    [TestCase(-1)]
    [TestCase(1)]
    public async Task fail_without_deleting_any_file_when_the_index_is_out_of_range(int index)
    {
        Chatbot chatbot = ChatbotMother.With(documents: [ChatbotDocumentMother.Random()]);
        this.ShouldFindChatbot(chatbot);

        Func<Task> act = async () => await this._handler.Handle(
            ChatbotDocumentRemoverCommandMother.With(chatbot.Id.Value, index),
            CancellationToken.None);

        await act.Should().ThrowAsync<ChatbotDocumentNotFoundError>();
        chatbot.Documents.Should().HaveCount(1);
        this.ShouldNotHaveDeletedAnyDocumentFile();
        this.ShouldNotHaveSavedAnyChatbot();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_deleting_any_file_when_the_chatbot_does_not_exist()
    {
        this.ShouldFindChatbot(null);

        Func<Task> act = async () =>
            await this._handler.Handle(ChatbotDocumentRemoverCommandMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<ChatbotNotFoundError>();
        this.ShouldNotHaveDeletedAnyDocumentFile();
        this.ShouldNotHaveSavedAnyChatbot();
        this.ShouldNotHavePublishedAnything();
    }
}
