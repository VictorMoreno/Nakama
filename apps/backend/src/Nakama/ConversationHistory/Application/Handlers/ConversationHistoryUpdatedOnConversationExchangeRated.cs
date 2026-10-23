using Nakama.Chatbots.Domain.Events;
using Nakama.ConversationHistory.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ConversationHistory.Application.Handlers;

using ConversationHistory = Nakama.ConversationHistory.Domain.ConversationHistory;

public sealed class ConversationHistoryUpdatedOnConversationExchangeRated(IConversationHistoryRepository conversationHistoryRepository)
    : DomainEventHandler<ConversationExchangeRated>
{
    protected override async Task Handle(ConversationExchangeRated conversationExchangeRated, CancellationToken cancellationToken)
    {
        ConversationHistory? history = await conversationHistoryRepository.FindById(conversationExchangeRated.BotKey.Value, cancellationToken);
        if (history is null)
        {
            return;
        }

        history.RateExchange(
            conversationExchangeRated.Id.Value,
            conversationExchangeRated.ExchangeIndex,
            conversationExchangeRated.Useful);
        await conversationHistoryRepository.Update(history, cancellationToken);
    }
}
