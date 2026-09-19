using Nakama.ChatPage.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatPage.Application.Handlers;

using ChatPage = Nakama.ChatPage.Domain.ChatPage;

public sealed class ChatPageUpdatedOnConversationExchangeAppended(IChatPageRepository chatPageRepository)
    : DomainEventHandler<ConversationExchangeAppended>
{
    protected override async Task Handle(ConversationExchangeAppended conversationExchangeAppended, CancellationToken ct)
    {
        var chatPage = await chatPageRepository.FindById(conversationExchangeAppended.BotKey.Value, ct);
        if (chatPage is null)
        {
            return;
        }

        chatPage.AppendExchange(
            conversationExchangeAppended.Id.Value,
            conversationExchangeAppended.Exchange.Question,
            conversationExchangeAppended.Exchange.Answer,
            conversationExchangeAppended.Exchange.Citation,
            conversationExchangeAppended.Exchange.MoreExplanation,
            conversationExchangeAppended.Exchange.SocraticVariant);
        await chatPageRepository.Update(chatPage, ct);
    }
}
