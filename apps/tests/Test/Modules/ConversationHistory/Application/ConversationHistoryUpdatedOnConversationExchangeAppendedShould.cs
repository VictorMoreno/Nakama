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
public sealed class ConversationHistoryUpdatedOnConversationExchangeAppendedShould : ConversationHistoryUnitTestCase
{
    private ConversationHistoryUpdatedOnConversationExchangeAppended _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ConversationHistoryUpdatedOnConversationExchangeAppended(this.HistoriesRepository.Object);
    }

    [Test]
    public async Task append_the_exchange_to_the_matching_conversation()
    {
        ConversationSummaryRow entry = ConversationSummaryRowMother.Random();
        ConversationHistoryEntity history = ConversationHistoryMother.With(entries: [entry]);
        ExchangeAttachment attachment = ExchangeAttachmentMother.Random();
        ConversationExchange exchange = ConversationExchangeMother.With(attachments: [attachment]);
        ConversationExchangeAppended @event = ConversationExchangeAppendedMother.With(
            ConversationId.From(entry.Id),
            ChatbotId.From(history.Id),
            exchange: exchange);
        this.ShouldFindConversationHistory(history);

        await this._handler.Handle(new DomainEventNotification<ConversationExchangeAppended>(@event), CancellationToken.None);

        HistoryExampleRow row = entry.Exchanges.Should().ContainSingle().Which;
        row.Question.Should().Be(exchange.Question);
        row.Answer.Should().Be(exchange.Answer);
        row.Attachments.Should().Equal(attachment.Name);
        this.ShouldHaveUpdatedConversationHistory(history);
    }

    [Test]
    public async Task record_no_attachments_when_the_exchange_has_none()
    {
        ConversationSummaryRow entry = ConversationSummaryRowMother.Random();
        ConversationHistoryEntity history = ConversationHistoryMother.With(entries: [entry]);
        ConversationExchangeAppended @event = ConversationExchangeAppendedMother.With(
            ConversationId.From(entry.Id),
            ChatbotId.From(history.Id),
            exchange: ConversationExchangeMother.With(attachments: null));
        this.ShouldFindConversationHistory(history);

        await this._handler.Handle(new DomainEventNotification<ConversationExchangeAppended>(@event), CancellationToken.None);

        entry.Exchanges.Should().ContainSingle().Which.Attachments.Should().BeEmpty();
    }

    [Test]
    public async Task leave_the_other_conversations_untouched()
    {
        ConversationSummaryRow entry = ConversationSummaryRowMother.Random();
        ConversationSummaryRow other = ConversationSummaryRowMother.Random();
        ConversationHistoryEntity history = ConversationHistoryMother.With(entries: [entry, other]);
        ConversationExchangeAppended @event = ConversationExchangeAppendedMother.With(
            ConversationId.From(entry.Id),
            ChatbotId.From(history.Id));
        this.ShouldFindConversationHistory(history);

        await this._handler.Handle(new DomainEventNotification<ConversationExchangeAppended>(@event), CancellationToken.None);

        other.Exchanges.Should().BeEmpty();
    }

    [Test]
    public async Task add_the_exchange_to_no_conversation_when_it_is_not_in_the_history()
    {
        ConversationSummaryRow entry = ConversationSummaryRowMother.Random();
        ConversationHistoryEntity history = ConversationHistoryMother.With(entries: [entry]);
        ConversationExchangeAppended @event = ConversationExchangeAppendedMother.With(botKey: ChatbotId.From(history.Id));
        this.ShouldFindConversationHistory(history);

        await this._handler.Handle(new DomainEventNotification<ConversationExchangeAppended>(@event), CancellationToken.None);

        entry.Exchanges.Should().BeEmpty();
    }

    [Test]
    public async Task do_nothing_when_the_history_does_not_exist()
    {
        ConversationExchangeAppended @event = ConversationExchangeAppendedMother.Random();
        this.ShouldNotFindConversationHistory();

        await this._handler.Handle(new DomainEventNotification<ConversationExchangeAppended>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyConversationHistory();
    }
}
