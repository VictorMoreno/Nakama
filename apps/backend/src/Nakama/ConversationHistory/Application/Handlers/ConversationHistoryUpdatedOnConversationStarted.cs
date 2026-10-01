using Nakama.ConversationHistory.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ConversationHistory.Application.Handlers;

public sealed class ConversationHistoryUpdatedOnConversationStarted(IConversationHistoryRepository conversationHistoryRepository)
    : DomainEventHandler<ConversationStarted>
{
    protected override async Task Handle(ConversationStarted conversationStarted, CancellationToken ct)
    {
        var history = await conversationHistoryRepository.FindById(conversationStarted.BotKey.Value, ct);
        if (history is null)
        {
            return;
        }

        history.StartConversation(conversationStarted.Id.Value, conversationStarted.BotKey.Value, conversationStarted.UserId, conversationStarted.Title);
        await conversationHistoryRepository.Update(history, ct);
    }
}
