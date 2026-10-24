using AwesomeAssertions;
using Nakama.Chatbots.Application.Unarchiver;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Nakama.Chatbots.Domain.Events;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.Chatbots.Application;

[TestFixture]
public sealed class ChatbotUnarchiverShould : ChatbotsUnitTestCase
{
    private ChatbotUnarchiver.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotUnarchiver.Handler(new(this.ChatbotsRepository.Object, this.EventBus.Object));
    }

    [Test]
    public async Task unarchive_the_chatbot_and_publish_chatbot_unarchived()
    {
        Chatbot chatbot = ChatbotMother.Archived();
        this.ShouldFindChatbot(chatbot);

        await this._handler.Handle(ChatbotUnarchiverCommandMother.With(chatbot.Id.Value), CancellationToken.None);

        chatbot.Archived.Should().BeFalse();
        this.ShouldHaveSavedChatbot(chatbot);
        this.ShouldHavePublished<ChatbotUnarchived>(@event => @event.Id == chatbot.Id);
    }

    [Test]
    public async Task record_no_new_event_when_the_chatbot_was_not_archived()
    {
        Chatbot chatbot = ChatbotMother.Random();
        this.ShouldFindChatbot(chatbot);

        await this._handler.Handle(ChatbotUnarchiverCommandMother.With(chatbot.Id.Value), CancellationToken.None);

        chatbot.Archived.Should().BeFalse();
        chatbot.DomainEvents.Should().BeEmpty();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_chatbot_does_not_exist()
    {
        this.ShouldNotFindChatbot();

        Func<Task> act = async () =>
            await this._handler.Handle(ChatbotUnarchiverCommandMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<ChatbotNotFoundError>();
        this.ShouldNotHaveSavedAnyChatbot();
        this.ShouldNotHavePublishedAnything();
    }
}
