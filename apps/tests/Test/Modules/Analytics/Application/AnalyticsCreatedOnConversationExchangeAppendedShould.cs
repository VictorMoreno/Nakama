using AwesomeAssertions;
using Nakama.Analytics.Application.Handlers;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.Analytics.Domain;
using Test.Modules.Chatbots.Domain;
using AnalyticsEntity = Nakama.Analytics.Domain.Analytics;

namespace Test.Modules.Analytics.Application;

[TestFixture]
public sealed class AnalyticsCreatedOnConversationExchangeAppendedShould : AnalyticsUnitTestCase
{
    private AnalyticsCreatedOnConversationExchangeAppended _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new AnalyticsCreatedOnConversationExchangeAppended(this.AnalyticsRepository.Object);
    }

    [Test]
    public async Task create_the_analytics_counting_the_answer_when_none_exist()
    {
        ConversationExchangeAppended @event = ConversationExchangeAppendedMother.Random();
        this.ShouldFindAnalytics(null);

        await this._handler.Handle(
            new DomainEventNotification<ConversationExchangeAppended>(@event),
            CancellationToken.None);

        this.ShouldHaveSavedAnalyticsMatching(analytics =>
            analytics.AnsweredThisWeek == 1
            && analytics.UsageByBot.Count == 1
            && analytics.UsageByBot[0].BotKey == @event.BotKey.Value
            && analytics.UsageByBot[0].Value == 1);
        this.ShouldNotHaveUpdatedAnyAnalytics();
    }

    [Test]
    public async Task count_the_answer_in_the_usage_row_of_the_chatbot_when_it_already_exists()
    {
        ConversationExchangeAppended @event = ConversationExchangeAppendedMother.Random();
        AnalyticsEntity analytics = AnalyticsMother.With(
            answeredThisWeek: 10,
            usageByBot: [UsageByBotRowMother.With(@event.BotKey.Value, 4), UsageByBotRowMother.With(value: 2)]);
        this.ShouldFindAnalytics(analytics);

        await this._handler.Handle(
            new DomainEventNotification<ConversationExchangeAppended>(@event),
            CancellationToken.None);

        analytics.AnsweredThisWeek.Should().Be(11);
        analytics.UsageByBot.Single(row => row.BotKey == @event.BotKey.Value).Value.Should().Be(5);
        analytics.UsageByBot.Single(row => row.BotKey != @event.BotKey.Value).Value.Should().Be(2);
        this.ShouldHaveUpdatedAnalytics(analytics);
        this.ShouldNotHaveSavedAnyAnalytics();
    }

    [Test]
    public async Task add_a_usage_row_for_the_chatbot_when_it_has_none_yet()
    {
        ConversationExchangeAppended @event = ConversationExchangeAppendedMother.Random();
        AnalyticsEntity analytics = AnalyticsMother.WithUsageRow(answeredThisWeek: 3);
        this.ShouldFindAnalytics(analytics);

        await this._handler.Handle(
            new DomainEventNotification<ConversationExchangeAppended>(@event),
            CancellationToken.None);

        analytics.AnsweredThisWeek.Should().Be(4);
        analytics.UsageByBot.Should().HaveCount(2);
        analytics.UsageByBot.Should().Contain(row => row.BotKey == @event.BotKey.Value && row.Value == 1);
        this.ShouldHaveUpdatedAnalytics(analytics);
    }
}
