using Nakama.ChatPage.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatPage.Application.Handlers;

public sealed class ChatPageUpdatedOnConversationStarted(IChatPageRepository chatPageRepository)
    : DomainEventHandler<ConversationStarted>
{
    protected override async Task Handle(ConversationStarted conversationStarted, CancellationToken ct)
    {
        var chatPage = await chatPageRepository.FindById(conversationStarted.BotKey.Value, ct);
        if (chatPage is null)
        {
            return;
        }

        chatPage.StartConversation(conversationStarted.Id.Value, conversationStarted.BotKey.Value, conversationStarted.Title);
        await chatPageRepository.Update(chatPage, ct);
    }
}
