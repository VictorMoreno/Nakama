using AwesomeAssertions;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.ConversationHistory.Application.Handlers;
using Nakama.ConversationHistory.Domain;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.ConversationHistory.Domain;
using ConversationHistoryEntity = Nakama.ConversationHistory.Domain.ConversationHistory;

namespace Test.Modules.ConversationHistory.Application;

[TestFixture]
public sealed class ConversationHistoryUpdatedOnConversationExchangeRatedShould : ConversationHistoryUnitTestCase
{
    private ConversationHistoryUpdatedOnConversationExchangeRated _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ConversationHistoryUpdatedOnConversationExchangeRated(this.HistoriesRepository.Object);
    }

    [Test]
    public async Task keep_the_rating_in_the_matching_exchange_row()
    {
        ConversationSummaryRow entry = ConversationSummaryRowMother.With(
            exchanges: [HistoryExampleRowMother.Random(), HistoryExampleRowMother.Random()]);
        ConversationHistoryEntity history = ConversationHistoryMother.With(entries: [entry]);
        this.ShouldFindConversationHistory(history);

        await this._handler.Handle(
            new DomainEventNotification<ConversationExchangeRated>(
                new ConversationExchangeRated(
                    ConversationId.From(entry.Id), ChatbotId.From(history.Id), entry.UserId, 1, "q", false, DateTime.UtcNow)),
            CancellationToken.None);

        entry.Exchanges.Select(row => row.Useful).Should().Equal(null, false);
        this.ShouldHaveUpdatedConversationHistory(history);
    }

    [Test]
    public async Task replace_a_previous_rating()
    {
        ConversationSummaryRow entry = ConversationSummaryRowMother.With(exchanges: [HistoryExampleRowMother.Random()]);
        entry.Exchanges[0].Useful = false;
        ConversationHistoryEntity history = ConversationHistoryMother.With(entries: [entry]);
        this.ShouldFindConversationHistory(history);

        await this._handler.Handle(
            new DomainEventNotification<ConversationExchangeRated>(
                new ConversationExchangeRated(
                    ConversationId.From(entry.Id), ChatbotId.From(history.Id), entry.UserId, 0, "q", true, DateTime.UtcNow)),
            CancellationToken.None);

        entry.Exchanges.Single().Useful.Should().BeTrue();
    }

    [Test]
    public async Task ignore_an_exchange_that_is_not_in_the_row()
    {
        ConversationSummaryRow entry = ConversationSummaryRowMother.With(exchanges: [HistoryExampleRowMother.Random()]);
        ConversationHistoryEntity history = ConversationHistoryMother.With(entries: [entry]);
        this.ShouldFindConversationHistory(history);

        await this._handler.Handle(
            new DomainEventNotification<ConversationExchangeRated>(
                new ConversationExchangeRated(
                    ConversationId.From(entry.Id), ChatbotId.From(history.Id), entry.UserId, 5, "q", true, DateTime.UtcNow)),
            CancellationToken.None);

        entry.Exchanges.Single().Useful.Should().BeNull();
    }

    [Test]
    public async Task do_nothing_when_the_chatbot_has_no_history()
    {
        this.ShouldNotFindConversationHistory();

        await this._handler.Handle(
            new DomainEventNotification<ConversationExchangeRated>(
                new ConversationExchangeRated(
                    ConversationId.From(Guid.NewGuid()), ChatbotId.From(Guid.NewGuid()), Guid.NewGuid(), 0, "q", true, DateTime.UtcNow)),
            CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyConversationHistory();
    }
}
