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
public sealed class ChatbotSummaryUpdatedOnChatbotDocumentAddedShould : ChatbotSummariesUnitTestCase
{
    private ChatbotSummaryUpdatedOnChatbotDocumentAdded _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotSummaryUpdatedOnChatbotDocumentAdded(this.SummariesRepository.Object);
    }

    [Test]
    public async Task count_one_more_document()
    {
        ChatbotSummary summary = ChatbotSummaryMother.With(docsCount: 2);
        ChatbotDocumentAdded @event = ChatbotDocumentAddedMother.With(ChatbotId.From(summary.Id));
        this.ShouldFindChatbotSummary(summary);

        await this._handler.Handle(new DomainEventNotification<ChatbotDocumentAdded>(@event), CancellationToken.None);

        summary.DocsCount.Should().Be(3);
        this.ShouldHaveUpdatedChatbotSummary(summary);
    }

    [Test]
    public async Task do_nothing_when_the_summary_does_not_exist()
    {
        ChatbotDocumentAdded @event = ChatbotDocumentAddedMother.Random();
        this.ShouldNotFindChatbotSummary();

        await this._handler.Handle(new DomainEventNotification<ChatbotDocumentAdded>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyChatbotSummary();
    }
}
