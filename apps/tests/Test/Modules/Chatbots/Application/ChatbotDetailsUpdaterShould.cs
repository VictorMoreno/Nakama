using AwesomeAssertions;
using Nakama.Chatbots.Application.DetailsUpdater;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Nakama.Chatbots.Domain.Events;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.Chatbots.Application;

[TestFixture]
public sealed class ChatbotDetailsUpdaterShould : ChatbotsUnitTestCase
{
    private ChatbotDetailsUpdater.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotDetailsUpdater.Handler(new(this.ChatbotsRepository.Object, this.EventBus.Object));
    }

    [Test]
    public async Task change_the_label_and_the_description_and_publish_chatbot_details_updated()
    {
        Chatbot chatbot = ChatbotMother.Random();
        string label = $"{chatbot.Label} updated";
        string description = $"{chatbot.Description} updated";
        this.ShouldFindChatbot(chatbot);

        await this._handler.Handle(
            ChatbotDetailsUpdaterCommandMother.With(chatbot.Id.Value, label, description),
            CancellationToken.None);

        chatbot.Label.Should().Be(label);
        chatbot.Description.Should().Be(description);
        this.ShouldHaveSavedChatbot(chatbot);
        this.ShouldHavePublished<ChatbotDetailsUpdated>(
            @event => @event.Id == chatbot.Id && @event.Label == label && @event.Description == description);
    }

    [Test]
    public async Task record_no_new_event_when_the_details_are_the_same()
    {
        Chatbot chatbot = ChatbotMother.Random();
        this.ShouldFindChatbot(chatbot);

        await this._handler.Handle(
            ChatbotDetailsUpdaterCommandMother.With(chatbot.Id.Value, chatbot.Label, chatbot.Description),
            CancellationToken.None);

        chatbot.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_chatbot_does_not_exist()
    {
        this.ShouldNotFindChatbot();

        Func<Task> act = async () =>
            await this._handler.Handle(ChatbotDetailsUpdaterCommandMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<ChatbotNotFoundError>();
        this.ShouldNotHaveSavedAnyChatbot();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_chatbot_is_archived()
    {
        Chatbot chatbot = ChatbotMother.Archived();
        this.ShouldFindChatbot(chatbot);

        Func<Task> act = async () =>
            await this._handler.Handle(ChatbotDetailsUpdaterCommandMother.With(chatbot.Id.Value), CancellationToken.None);

        await act.Should().ThrowAsync<ChatbotArchivedError>();
        this.ShouldNotHaveSavedAnyChatbot();
        this.ShouldNotHavePublishedAnything();
    }
}
