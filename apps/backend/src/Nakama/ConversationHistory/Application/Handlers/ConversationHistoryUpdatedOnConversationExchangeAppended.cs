using Nakama.ConversationHistory.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ConversationHistory.Application.Handlers;

using ConversationHistory = Nakama.ConversationHistory.Domain.ConversationHistory;

public sealed class ConversationHistoryUpdatedOnConversationExchangeAppended(IConversationHistoryRepository conversationHistoryRepository)
    : DomainEventHandler<ConversationExchangeAppended>
{
    protected override async Task Handle(ConversationExchangeAppended conversationExchangeAppended, CancellationToken cancellationToken)
    {
        ConversationHistory? history = await conversationHistoryRepository.FindById(conversationExchangeAppended.BotKey.Value, cancellationToken);
        if (history is null)
        {
            return;
        }

        history.AppendExchange(
            conversationExchangeAppended.Id.Value,
            conversationExchangeAppended.Exchange.Question,
            conversationExchangeAppended.Exchange.Answer,
            (conversationExchangeAppended.Exchange.Attachments ?? []).Select(a => a.Name));
        await conversationHistoryRepository.Update(history, cancellationToken);
    }
}
