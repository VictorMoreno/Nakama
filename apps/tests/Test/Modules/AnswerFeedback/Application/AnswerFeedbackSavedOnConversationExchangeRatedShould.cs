using AwesomeAssertions;
using Nakama.AnswerFeedback.Application.Handlers;
using Nakama.AnswerFeedback.Domain;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.AnswerFeedback.Application;

[TestFixture]
public sealed class AnswerFeedbackSavedOnConversationExchangeRatedShould : AnswerFeedbackUnitTestCase
{
    private AnswerFeedbackSavedOnConversationExchangeRated _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new AnswerFeedbackSavedOnConversationExchangeRated(this.FeedbackRepository.Object);
    }

    [Test]
    public async Task save_the_rating_of_an_exchange_that_was_not_rated_before()
    {
        ConversationExchangeRated @event = EventFor(ConversationMother.RandomId(), 2, useful: false);
        this.ShouldNotFindFeedback();

        await this._handler.Handle(new DomainEventNotification<ConversationExchangeRated>(@event), CancellationToken.None);

        this.ShouldHaveSavedFeedbackMatching(entry =>
            entry.Id == AnswerFeedbackEntry.IdOf(@event.Id.Value, 2)
            && entry.BotKey == @event.BotKey.Value
            && entry.Question == @event.Question
            && !entry.Useful
            && entry.RatedAt == @event.OccurredAt);
    }

    [Test]
    public async Task replace_the_previous_rating_of_the_same_exchange()
    {
        ConversationExchangeRated @event = EventFor(ConversationMother.RandomId(), 0, useful: true);
        AnswerFeedbackEntry existing = AnswerFeedbackEntry.Create(
            @event.Id.Value, 0, @event.BotKey.Value, @event.Question, false, @event.OccurredAt.AddMinutes(-5));
        this.ShouldFindFeedback(existing);

        await this._handler.Handle(new DomainEventNotification<ConversationExchangeRated>(@event), CancellationToken.None);

        existing.Useful.Should().BeTrue();
        existing.RatedAt.Should().Be(@event.OccurredAt);
        this.ShouldHaveSavedFeedbackMatching(entry => entry == existing);
    }

    [Test]
    public void give_each_exchange_of_each_conversation_its_own_stable_id()
    {
        Guid conversation = Guid.NewGuid();

        AnswerFeedbackEntry.IdOf(conversation, 0).Should().Be(AnswerFeedbackEntry.IdOf(conversation, 0));
        AnswerFeedbackEntry.IdOf(conversation, 0).Should().NotBe(AnswerFeedbackEntry.IdOf(conversation, 1));
        AnswerFeedbackEntry.IdOf(conversation, 0).Should().NotBe(AnswerFeedbackEntry.IdOf(Guid.NewGuid(), 0));
    }

    private static ConversationExchangeRated EventFor(ConversationId id, int index, bool useful) =>
        new(id, ChatbotId.From(Guid.NewGuid()), Guid.NewGuid(), index, "How do I deploy?", useful, DateTime.UtcNow);
}
