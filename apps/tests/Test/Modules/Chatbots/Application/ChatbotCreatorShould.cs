using AwesomeAssertions;
using Nakama.Chatbots.Application.Creator;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Nakama.Chatbots.Domain.Events;
using Nakama.LlmProviders.Domain;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.Chatbots.Application;

[TestFixture]
public sealed class ChatbotCreatorShould : ChatbotsUnitTestCase
{
    private ChatbotCreator.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotCreator.Handler(new(this.ChatbotsRepository.Object, this.EventBus.Object));
    }

    [Test]
    public async Task create_the_chatbot_and_publish_chatbot_created_when_it_does_not_exist()
    {
        ChatbotCreator.Command command = ChatbotCreatorCommandMother.Random();
        ChatbotSettings settings = new(
            command.Socratic,
            command.ExplicaMas,
            (LlmProvider)command.Provider,
            command.Model);
        this.ShouldFindChatbot(null);

        await this._handler.Handle(command, CancellationToken.None);

        this.ShouldHaveSavedChatbotMatching(chatbot =>
            chatbot.Id.Value == command.Id
            && chatbot.Label == command.Label
            && chatbot.Description == command.Description
            && chatbot.Status == (ChatbotStatus)command.Status
            && chatbot.Settings == settings);
        this.ShouldHavePublished<ChatbotCreated>(@event => @event.Id.Value == command.Id);
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_chatbot_already_exists()
    {
        ChatbotCreator.Command command = ChatbotCreatorCommandMother.Random();
        this.ShouldFindChatbot(ChatbotMother.With(id: ChatbotId.From(command.Id)));

        Func<Task> act = async () => await this._handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ChatbotAlreadyExistsError>();
        this.ShouldNotHaveSavedAnyChatbot();
        this.ShouldNotHavePublishedAnything();
    }
}
