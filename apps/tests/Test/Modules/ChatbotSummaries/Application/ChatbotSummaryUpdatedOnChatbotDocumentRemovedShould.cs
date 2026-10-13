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
public sealed class ChatbotSummaryUpdatedOnChatbotDocumentRemovedShould : ChatbotSummariesUnitTestCase
{
    private ChatbotSummaryUpdatedOnChatbotDocumentRemoved _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotSummaryUpdatedOnChatbotDocumentRemoved(this.SummariesRepository.Object);
    }

    [Test]
    public async Task count_one_document_less()
    {
        ChatbotSummary summary = ChatbotSummaryMother.With(docsCount: 3);
        ChatbotDocumentRemoved @event = ChatbotDocumentRemovedMother.With(ChatbotId.From(summary.Id));
        this.ShouldFindChatbotSummary(summary);

        await this._handler.Handle(new DomainEventNotification<ChatbotDocumentRemoved>(@event), CancellationToken.None);

        summary.DocsCount.Should().Be(2);
        this.ShouldHaveUpdatedChatbotSummary(summary);
    }

    [Test]
    public async Task keep_the_document_count_at_zero_when_there_are_no_documents()
    {
        ChatbotSummary summary = ChatbotSummaryMother.With(docsCount: 0);
        ChatbotDocumentRemoved @event = ChatbotDocumentRemovedMother.With(ChatbotId.From(summary.Id));
        this.ShouldFindChatbotSummary(summary);

        await this._handler.Handle(new DomainEventNotification<ChatbotDocumentRemoved>(@event), CancellationToken.None);

        summary.DocsCount.Should().Be(0);
    }

    [Test]
    public async Task do_nothing_when_the_summary_does_not_exist()
    {
        ChatbotDocumentRemoved @event = ChatbotDocumentRemovedMother.Random();
        this.ShouldNotFindChatbotSummary();

        await this._handler.Handle(new DomainEventNotification<ChatbotDocumentRemoved>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyChatbotSummary();
    }
}
