using AwesomeAssertions;
using Nakama.Chatbots.Application.ExchangeRater;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Nakama.Chatbots.Domain.Events;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.Chatbots.Application;

[TestFixture]
public sealed class ConversationExchangeRaterShould : ChatbotsUnitTestCase
{
    private static readonly DateTime Now = new(2026, 10, 22, 10, 0, 0, DateTimeKind.Utc);

    private ConversationExchangeRater.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ConversationExchangeRater.Handler(
            new(this.ConversationsRepository.Object, this.EventBus.Object, this.Clock.Object));
        this.ShouldBeNow(Now);
    }

    [Test]
    public async Task keep_the_rating_in_the_exchange_and_publish_that_it_was_rated()
    {
        Conversation conversation = ConversationMother.With(exchanges:
        [
            ConversationExchangeMother.Random(),
            ConversationExchangeMother.With(question: "How do I deploy?")
        ]);
        this.ShouldFindConversation(conversation);

        await this._handler.Handle(
            ConversationExchangeRaterCommandMother.With(
                conversation.BotKey.Value, conversation.Id.Value, exchangeIndex: 1, useful: false, userId: conversation.UserId),
            CancellationToken.None);

        conversation.Exchanges.Select(exchange => exchange.Useful).Should().Equal(null, false);
        this.ShouldHaveSavedConversation(conversation);
        this.ShouldHavePublished<ConversationExchangeRated>(@event =>
            @event.Id == conversation.Id
            && @event.BotKey == conversation.BotKey
            && @event.ExchangeIndex == 1
            && @event.Question == "How do I deploy?"
            && !@event.Useful
            && @event.OccurredAt == Now);
    }

    [Test]
    public async Task let_the_person_change_the_rating_of_an_exchange_they_already_rated()
    {
        Conversation conversation = ConversationMother.With(exchanges: [ConversationExchangeMother.With(useful: false)]);
        this.ShouldFindConversation(conversation);

        await this._handler.Handle(
            ConversationExchangeRaterCommandMother.With(
                conversation.BotKey.Value, conversation.Id.Value, exchangeIndex: 0, useful: true, userId: conversation.UserId),
            CancellationToken.None);

        conversation.Exchanges.Single().Useful.Should().BeTrue();
        this.ShouldHavePublished<ConversationExchangeRated>(@event => @event.Useful);
    }

    [Test]
    public async Task fail_when_the_conversation_does_not_exist()
    {
        this.ShouldNotFindConversation();

        Func<Task> act = async () =>
            await this._handler.Handle(ConversationExchangeRaterCommandMother.With(), CancellationToken.None);

        await act.Should().ThrowAsync<ConversationNotFoundError>();
        this.ShouldNotHaveSavedAnyConversation();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_when_the_conversation_belongs_to_another_person()
    {
        Conversation conversation = ConversationMother.With(exchanges: [ConversationExchangeMother.Random()]);
        this.ShouldFindConversation(conversation);

        Func<Task> act = async () => await this._handler.Handle(
            ConversationExchangeRaterCommandMother.With(conversation.BotKey.Value, conversation.Id.Value),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConversationNotFoundError>();
        conversation.Exchanges.Single().Useful.Should().BeNull();
        this.ShouldNotHaveSavedAnyConversation();
    }

    [Test]
    public async Task fail_when_the_conversation_is_of_another_chatbot()
    {
        Conversation conversation = ConversationMother.With(exchanges: [ConversationExchangeMother.Random()]);
        this.ShouldFindConversation(conversation);

        Func<Task> act = async () => await this._handler.Handle(
            ConversationExchangeRaterCommandMother.With(Guid.NewGuid(), conversation.Id.Value, userId: conversation.UserId),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConversationNotFoundError>();
        this.ShouldNotHaveSavedAnyConversation();
    }

    [TestCase(-1)]
    [TestCase(1)]
    public async Task fail_when_the_exchange_does_not_exist(int exchangeIndex)
    {
        Conversation conversation = ConversationMother.With(exchanges: [ConversationExchangeMother.Random()]);
        this.ShouldFindConversation(conversation);

        Func<Task> act = async () => await this._handler.Handle(
            ConversationExchangeRaterCommandMother.With(
                conversation.BotKey.Value, conversation.Id.Value, exchangeIndex, userId: conversation.UserId),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConversationExchangeNotFoundError>();
        this.ShouldNotHaveSavedAnyConversation();
        this.ShouldNotHavePublishedAnything();
    }
}
