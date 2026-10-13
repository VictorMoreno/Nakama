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
public sealed class ChatbotSummaryUpdatedOnChatbotStatusChangedShould : ChatbotSummariesUnitTestCase
{
    private ChatbotSummaryUpdatedOnChatbotStatusChanged _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotSummaryUpdatedOnChatbotStatusChanged(this.SummariesRepository.Object);
    }

    [Test]
    public async Task replace_the_status_of_the_summary()
    {
        ChatbotSummary summary = ChatbotSummaryMother.With(status: ChatbotStatus.Active.ToString());
        ChatbotStatusChanged @event = ChatbotStatusChangedMother.With(ChatbotId.From(summary.Id), ChatbotStatus.Draft);
        this.ShouldFindChatbotSummary(summary);

        await this._handler.Handle(new DomainEventNotification<ChatbotStatusChanged>(@event), CancellationToken.None);

        summary.Status.Should().Be(ChatbotStatus.Draft.ToString());
        this.ShouldHaveUpdatedChatbotSummary(summary);
    }

    [Test]
    public async Task do_nothing_when_the_summary_does_not_exist()
    {
        ChatbotStatusChanged @event = ChatbotStatusChangedMother.Random();
        this.ShouldNotFindChatbotSummary();

        await this._handler.Handle(new DomainEventNotification<ChatbotStatusChanged>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyChatbotSummary();
    }
}
