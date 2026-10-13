using AwesomeAssertions;
using Nakama.Chatbots.Application.StatusUpdater;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Nakama.Chatbots.Domain.Events;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.Chatbots.Application;

[TestFixture]
public sealed class ChatbotStatusUpdaterShould : ChatbotsUnitTestCase
{
    private ChatbotStatusUpdater.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotStatusUpdater.Handler(new(this.ChatbotsRepository.Object, this.EventBus.Object));
    }

    [Test]
    public async Task change_the_status_and_publish_chatbot_status_changed()
    {
        Chatbot chatbot = ChatbotMother.With(status: ChatbotStatus.Draft);
        this.ShouldFindChatbot(chatbot);

        await this._handler.Handle(
            ChatbotStatusUpdaterCommandMother.With(chatbot.Id.Value, ChatbotStatus.Active),
            CancellationToken.None);

        chatbot.Status.Should().Be(ChatbotStatus.Active);
        this.ShouldHaveSavedChatbot(chatbot);
        this.ShouldHavePublished<ChatbotStatusChanged>(
            @event => @event.Id == chatbot.Id && @event.Status == ChatbotStatus.Active);
    }

    [Test]
    public async Task record_no_new_event_when_the_status_is_the_same()
    {
        Chatbot chatbot = ChatbotMother.With(status: ChatbotStatus.Active);
        this.ShouldFindChatbot(chatbot);

        await this._handler.Handle(
            ChatbotStatusUpdaterCommandMother.With(chatbot.Id.Value, ChatbotStatus.Active),
            CancellationToken.None);

        chatbot.Status.Should().Be(ChatbotStatus.Active);
        chatbot.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_chatbot_does_not_exist()
    {
        this.ShouldNotFindChatbot();

        Func<Task> act = async () =>
            await this._handler.Handle(ChatbotStatusUpdaterCommandMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<ChatbotNotFoundError>();
        this.ShouldNotHaveSavedAnyChatbot();
        this.ShouldNotHavePublishedAnything();
    }
}
