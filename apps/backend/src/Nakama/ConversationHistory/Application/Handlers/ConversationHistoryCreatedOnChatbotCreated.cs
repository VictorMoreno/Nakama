using Nakama.ConversationHistory.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ConversationHistory.Application.Handlers;

using ConversationHistory = Nakama.ConversationHistory.Domain.ConversationHistory;

public sealed class ConversationHistoryCreatedOnChatbotCreated(IConversationHistoryRepository conversationHistoryRepository)
    : DomainEventHandler<ChatbotCreated>
{
    protected override Task Handle(ChatbotCreated chatbotCreated, CancellationToken cancellationToken) =>
        conversationHistoryRepository.Save(ConversationHistory.Create(chatbotCreated.Id.Value), cancellationToken);
}
