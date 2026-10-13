using AwesomeAssertions;
using Nakama.ChatbotSummaries.Application.Handlers;
using Nakama.ChatbotSummaries.Domain;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.ChatbotSummaries.Domain;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.ChatbotSummaries.Application;

[TestFixture]
public sealed class ChatbotSummaryUpdatedOnChatbotIconUpdatedShould : ChatbotSummariesUnitTestCase
{
    private ChatbotSummaryUpdatedOnChatbotIconUpdated _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotSummaryUpdatedOnChatbotIconUpdated(this.SummariesRepository.Object);
    }

    [Test]
    public async Task set_the_icon_of_the_summary()
    {
        ChatbotSummary summary = ChatbotSummaryMother.With(iconUrl: null);
        ChatbotIconUpdated @event = ChatbotIconUpdatedMother.With(ChatbotId.From(summary.Id));
        this.ShouldFindChatbotSummary(summary);

        await this._handler.Handle(new DomainEventNotification<ChatbotIconUpdated>(@event), CancellationToken.None);

        summary.IconUrl.Should().Be(@event.IconUrl);
        this.ShouldHaveUpdatedChatbotSummary(summary);
    }

    [Test]
    public async Task clear_the_icon_of_the_summary_when_the_event_has_none()
    {
        ChatbotSummary summary = ChatbotSummaryMother.With(iconUrl: "icon.png");
        ChatbotIconUpdated @event = ChatbotIconUpdatedMother.Cleared(ChatbotId.From(summary.Id));
        this.ShouldFindChatbotSummary(summary);

        await this._handler.Handle(new DomainEventNotification<ChatbotIconUpdated>(@event), CancellationToken.None);

        summary.IconUrl.Should().BeNull();
        this.ShouldHaveUpdatedChatbotSummary(summary);
    }

    [Test]
    public async Task do_nothing_when_the_summary_does_not_exist()
    {
        ChatbotIconUpdated @event = ChatbotIconUpdatedMother.Random();
        this.ShouldNotFindChatbotSummary();

        await this._handler.Handle(new DomainEventNotification<ChatbotIconUpdated>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyChatbotSummary();
    }
}
