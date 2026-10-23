using Nakama.Chatbots.Domain.Events;
using Nakama.QuestionActivity.Application.Handlers;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.QuestionActivity.Application;

[TestFixture]
public sealed class QuestionActivityEntryCreatedOnConversationExchangeAppendedShould : QuestionActivityUnitTestCase
{
    private QuestionActivityEntryCreatedOnConversationExchangeAppended _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new QuestionActivityEntryCreatedOnConversationExchangeAppended(this.QuestionRepository.Object);
    }

    [Test]
    public async Task save_an_entry_for_the_question_with_its_chatbot_person_and_time()
    {
        ConversationExchangeAppended @event = ConversationExchangeAppendedMother.Random();

        await this._handler.Handle(new DomainEventNotification<ConversationExchangeAppended>(@event), CancellationToken.None);

        this.ShouldHaveSavedQuestionActivityEntryMatching(entry =>
            entry.Id != Guid.Empty
            && entry.BotKey == @event.BotKey.Value
            && entry.UserId == @event.UserId
            && entry.OccurredAt == @event.OccurredAt);
    }
}
