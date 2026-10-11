using Nakama.ConversationHistory.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ConversationHistory.Application.Handlers;

public sealed class ConversationHistoryUpdatedOnConversationStarted(IConversationHistoryRepository conversationHistoryRepository)
    : DomainEventHandler<ConversationStarted>
{
    protected override async Task Handle(ConversationStarted conversationStarted, CancellationToken cancellationToken)
    {
        Domain.ConversationHistory? history =
            await conversationHistoryRepository.FindById(conversationStarted.BotKey.Value, cancellationToken);
        if (history is null)
        {
            return;
        }

        history.StartConversation(conversationStarted.Id.Value, conversationStarted.BotKey.Value, conversationStarted.UserId,
            conversationStarted.Title, conversationStarted.StartedAt);

        await conversationHistoryRepository.Update(history, cancellationToken);
    }
}
