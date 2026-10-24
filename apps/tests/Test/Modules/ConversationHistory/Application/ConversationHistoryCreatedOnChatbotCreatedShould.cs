using Nakama.Chatbots.Domain.Events;
using Nakama.ConversationHistory.Application.Handlers;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.ConversationHistory.Application;

[TestFixture]
public sealed class ConversationHistoryCreatedOnChatbotCreatedShould : ConversationHistoryUnitTestCase
{
    private ConversationHistoryCreatedOnChatbotCreated _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ConversationHistoryCreatedOnChatbotCreated(this.HistoriesRepository.Object);
    }

    [Test]
    public async Task save_an_empty_history_for_the_chatbot()
    {
        ChatbotCreated @event = ChatbotCreatedMother.Random();

        await this._handler.Handle(new DomainEventNotification<ChatbotCreated>(@event), CancellationToken.None);

        this.ShouldHaveSavedConversationHistoryMatching(history =>
            history.Id == @event.Id.Value && history.Entries.Count == 0);
        this.ShouldNotHaveUpdatedAnyConversationHistory();
    }
}
