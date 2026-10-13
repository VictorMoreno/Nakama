using AwesomeAssertions;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.ConversationHistory.Application.Handlers;
using Nakama.ConversationHistory.Domain;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.Chatbots.Domain;
using Test.Modules.ConversationHistory.Domain;
using ConversationHistoryEntity = Nakama.ConversationHistory.Domain.ConversationHistory;

namespace Test.Modules.ConversationHistory.Application;

[TestFixture]
public sealed class ConversationHistoryUpdatedOnConversationDeletedShould : ConversationHistoryUnitTestCase
{
    private ConversationHistoryUpdatedOnConversationDeleted _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ConversationHistoryUpdatedOnConversationDeleted(this.HistoriesRepository.Object);
    }

    [Test]
    public async Task remove_the_conversation_from_the_history()
    {
        ConversationSummaryRow removed = ConversationSummaryRowMother.Random();
        ConversationSummaryRow kept = ConversationSummaryRowMother.Random();
        ConversationHistoryEntity history = ConversationHistoryMother.With(entries: [removed, kept]);
        ConversationDeleted @event = ConversationDeletedMother.With(
            ConversationId.From(removed.Id),
            ChatbotId.From(history.Id));
        this.ShouldFindConversationHistory(history);

        await this._handler.Handle(new DomainEventNotification<ConversationDeleted>(@event), CancellationToken.None);

        history.Entries.Should().ContainSingle().Which.Should().BeSameAs(kept);
        this.ShouldHaveUpdatedConversationHistory(history);
    }

    [Test]
    public async Task leave_the_history_untouched_when_the_conversation_is_not_in_it()
    {
        ConversationSummaryRow entry = ConversationSummaryRowMother.Random();
        ConversationHistoryEntity history = ConversationHistoryMother.With(entries: [entry]);
        ConversationDeleted @event = ConversationDeletedMother.With(botKey: ChatbotId.From(history.Id));
        this.ShouldFindConversationHistory(history);

        await this._handler.Handle(new DomainEventNotification<ConversationDeleted>(@event), CancellationToken.None);

        history.Entries.Should().ContainSingle().Which.Should().BeSameAs(entry);
    }

    [Test]
    public async Task do_nothing_when_the_history_does_not_exist()
    {
        ConversationDeleted @event = ConversationDeletedMother.Random();
        this.ShouldNotFindConversationHistory();

        await this._handler.Handle(new DomainEventNotification<ConversationDeleted>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyConversationHistory();
    }
}
