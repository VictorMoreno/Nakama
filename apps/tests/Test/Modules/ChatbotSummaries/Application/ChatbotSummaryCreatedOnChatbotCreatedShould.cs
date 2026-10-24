using Nakama.ChatbotSummaries.Application.Handlers;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.ChatbotSummaries.Application;

[TestFixture]
public sealed class ChatbotSummaryCreatedOnChatbotCreatedShould : ChatbotSummariesUnitTestCase
{
    private ChatbotSummaryCreatedOnChatbotCreated _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotSummaryCreatedOnChatbotCreated(this.SummariesRepository.Object);
    }

    [Test]
    public async Task save_a_summary_with_the_data_of_the_chatbot_and_no_documents_or_team()
    {
        ChatbotCreated @event = ChatbotCreatedMother.Random();

        await this._handler.Handle(new DomainEventNotification<ChatbotCreated>(@event), CancellationToken.None);

        this.ShouldHaveSavedChatbotSummaryMatching(summary =>
            summary.Id == @event.Id.Value
            && summary.Label == @event.Label
            && summary.Description == @event.Description
            && summary.Status == @event.Status.ToString()
            && summary.DocsCount == 0
            && summary.TeamCount == 0
            && !summary.Archived
            && summary.IconUrl == null);
        this.ShouldNotHaveUpdatedAnyChatbotSummary();
    }
}
