using MediatR;
using Nakama.ChatPage.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatPage.Application.Handlers;

using ChatPage = Nakama.ChatPage.Domain.ChatPage;

/// <summary>Updates <see cref="ChatPage"/> from <see cref="ConversationStarted"/>.</summary>
public sealed class ChatPageUpdatedOnConversationStarted(IChatPageRepository chatPageRepository)
    : INotificationHandler<DomainEventNotification<ConversationStarted>>
{
    public async Task Handle(DomainEventNotification<ConversationStarted> n, CancellationToken ct)
    {
        var entry = await chatPageRepository.FindByBotKey(n.DomainEvent.BotKey.Value, ct);
        if (entry is null)
        {
            return;
        }

        entry.StartConversation(n.DomainEvent.Id.Value, n.DomainEvent.BotKey.Value, n.DomainEvent.Title);
        await chatPageRepository.Save(entry, ct);
    }
}
