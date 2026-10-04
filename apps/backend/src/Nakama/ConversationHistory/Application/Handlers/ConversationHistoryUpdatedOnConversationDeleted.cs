using Nakama.ConversationHistory.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ConversationHistory.Application.Handlers;

public sealed class ConversationHistoryUpdatedOnConversationDeleted(IConversationHistoryRepository conversationHistoryRepository)
    : DomainEventHandler<ConversationDeleted>
{
    protected override async Task Handle(ConversationDeleted conversationDeleted, CancellationToken cancellationToken)
    {
        var history = await conversationHistoryRepository.FindById(conversationDeleted.BotKey.Value, cancellationToken);
        if (history is null)
        {
            return;
        }

        history.RemoveConversation(conversationDeleted.Id.Value);
        await conversationHistoryRepository.Update(history, cancellationToken);
    }
}
