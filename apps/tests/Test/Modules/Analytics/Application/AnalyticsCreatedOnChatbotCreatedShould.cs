using AwesomeAssertions;
using Nakama.Analytics.Application.Handlers;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.Analytics.Domain;
using Test.Modules.Chatbots.Domain;
using AnalyticsEntity = Nakama.Analytics.Domain.Analytics;

namespace Test.Modules.Analytics.Application;

[TestFixture]
public sealed class AnalyticsCreatedOnChatbotCreatedShould : AnalyticsUnitTestCase
{
    private AnalyticsCreatedOnChatbotCreated _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new AnalyticsCreatedOnChatbotCreated(this.AnalyticsRepository.Object);
    }

    [Test]
    public async Task create_the_analytics_counting_the_chatbot_when_none_exist()
    {
        ChatbotCreated @event = ChatbotCreatedMother.Random();
        this.ShouldFindAnalytics(null);

        await this._handler.Handle(new DomainEventNotification<ChatbotCreated>(@event), CancellationToken.None);

        this.ShouldHaveSavedAnalyticsMatching(analytics =>
            analytics.ActiveBotsCount == 1
            && analytics.UsageByBot.Count == 1
            && analytics.UsageByBot[0].BotKey == @event.Id.Value
            && analytics.UsageByBot[0].Value == 0);
        this.ShouldNotHaveUpdatedAnyAnalytics();
    }

    [Test]
    public async Task count_the_chatbot_and_add_its_usage_row_in_the_existing_analytics()
    {
        AnalyticsEntity analytics = AnalyticsMother.With(activeBotsCount: 2, usageByBot: [UsageByBotRowMother.Random()]);
        ChatbotCreated @event = ChatbotCreatedMother.Random();
        this.ShouldFindAnalytics(analytics);

        await this._handler.Handle(new DomainEventNotification<ChatbotCreated>(@event), CancellationToken.None);

        analytics.ActiveBotsCount.Should().Be(3);
        analytics.UsageByBot.Should().HaveCount(2);
        analytics.UsageByBot.Should().Contain(row => row.BotKey == @event.Id.Value && row.Value == 0);
        this.ShouldHaveUpdatedAnalytics(analytics);
        this.ShouldNotHaveSavedAnyAnalytics();
    }

    [Test]
    public async Task not_duplicate_the_usage_row_when_the_chatbot_already_has_one()
    {
        ChatbotCreated @event = ChatbotCreatedMother.Random();
        AnalyticsEntity analytics = AnalyticsMother.With(
            activeBotsCount: 1,
            usageByBot: [UsageByBotRowMother.With(@event.Id.Value, 9)]);
        this.ShouldFindAnalytics(analytics);

        await this._handler.Handle(new DomainEventNotification<ChatbotCreated>(@event), CancellationToken.None);

        analytics.ActiveBotsCount.Should().Be(2);
        analytics.UsageByBot.Should().ContainSingle().Which.Value.Should().Be(9);
    }
}
