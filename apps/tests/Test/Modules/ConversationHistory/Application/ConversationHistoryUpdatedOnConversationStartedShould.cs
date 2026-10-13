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
public sealed class ConversationHistoryUpdatedOnConversationStartedShould : ConversationHistoryUnitTestCase
{
    private ConversationHistoryUpdatedOnConversationStarted _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ConversationHistoryUpdatedOnConversationStarted(this.HistoriesRepository.Object);
    }

    [Test]
    public async Task add_the_conversation_to_the_history_of_the_chatbot()
    {
        ConversationHistoryEntity history = ConversationHistoryMother.Random();
        ConversationStarted @event = ConversationStartedMother.With(botKey: ChatbotId.From(history.Id));
        this.ShouldFindConversationHistory(history);

        await this._handler.Handle(new DomainEventNotification<ConversationStarted>(@event), CancellationToken.None);

        ConversationSummaryRow entry = history.Entries.Should().ContainSingle().Which;
        entry.Id.Should().Be(@event.Id.Value);
        entry.BotKey.Should().Be(@event.BotKey.Value);
        entry.UserId.Should().Be(@event.UserId);
        entry.Title.Should().Be(@event.Title);
        entry.DateLabel.Should().Be(@event.StartedAt);
        entry.Exchanges.Should().BeEmpty();
        this.ShouldHaveUpdatedConversationHistory(history);
    }

    [Test]
    public async Task keep_the_conversations_already_in_the_history()
    {
        ConversationSummaryRow existing = ConversationSummaryRowMother.Random();
        ConversationHistoryEntity history = ConversationHistoryMother.With(entries: [existing]);
        ConversationStarted @event = ConversationStartedMother.With(botKey: ChatbotId.From(history.Id));
        this.ShouldFindConversationHistory(history);

        await this._handler.Handle(new DomainEventNotification<ConversationStarted>(@event), CancellationToken.None);

        history.Entries.Should().HaveCount(2);
        history.Entries.First().Should().BeSameAs(existing);
    }

    [Test]
    public async Task do_nothing_when_the_history_does_not_exist()
    {
        ConversationStarted @event = ConversationStartedMother.Random();
        this.ShouldNotFindConversationHistory();

        await this._handler.Handle(new DomainEventNotification<ConversationStarted>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyConversationHistory();
    }
}
