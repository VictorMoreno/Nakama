using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotDetail.Application.Handlers;

using ChatbotDetail = Nakama.ChatbotDetail.Domain.ChatbotDetail;

public sealed class ChatbotDetailUpdatedOnConversationExchangeAppended(IChatbotDetailRepository chatbotDetailRepository)
    : DomainEventHandler<ConversationExchangeAppended>
{
    protected override async Task Handle(ConversationExchangeAppended conversationExchangeAppended, CancellationToken ct)
    {
        var chatbotDetail = await chatbotDetailRepository.FindById(conversationExchangeAppended.BotKey.Value, ct);
        if (chatbotDetail is null)
        {
            return;
        }

        chatbotDetail.RecordQuestion(conversationExchangeAppended.Exchange.Question);
        await chatbotDetailRepository.Update(chatbotDetail, ct);
    }
}
