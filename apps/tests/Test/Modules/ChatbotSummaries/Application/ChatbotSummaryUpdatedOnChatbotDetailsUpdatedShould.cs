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
public sealed class ChatbotSummaryUpdatedOnChatbotDetailsUpdatedShould : ChatbotSummariesUnitTestCase
{
    private ChatbotSummaryUpdatedOnChatbotDetailsUpdated _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotSummaryUpdatedOnChatbotDetailsUpdated(this.SummariesRepository.Object);
    }

    [Test]
    public async Task replace_the_label_and_the_description_of_the_summary()
    {
        ChatbotSummary summary = ChatbotSummaryMother.With();
        string label = $"{summary.Label} updated";
        string description = $"{summary.Description} updated";
        ChatbotDetailsUpdated @event = ChatbotDetailsUpdatedMother.With(ChatbotId.From(summary.Id), label, description);
        this.ShouldFindChatbotSummary(summary);

        await this._handler.Handle(new DomainEventNotification<ChatbotDetailsUpdated>(@event), CancellationToken.None);

        summary.Label.Should().Be(label);
        summary.Description.Should().Be(description);
        this.ShouldHaveUpdatedChatbotSummary(summary);
    }

    [Test]
    public async Task do_nothing_when_the_summary_does_not_exist()
    {
        ChatbotDetailsUpdated @event = ChatbotDetailsUpdatedMother.Random();
        this.ShouldNotFindChatbotSummary();

        await this._handler.Handle(new DomainEventNotification<ChatbotDetailsUpdated>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyChatbotSummary();
    }
}
