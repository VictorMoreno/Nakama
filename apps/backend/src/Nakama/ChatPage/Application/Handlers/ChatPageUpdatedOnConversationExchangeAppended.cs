using MediatR;
using Nakama.ChatPage.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatPage.Application.Handlers;

using ChatPage = Nakama.ChatPage.Domain.ChatPage;

/// <summary>Updates <see cref="ChatPage"/> from <see cref="ConversationExchangeAppended"/>.</summary>
public sealed class ChatPageUpdatedOnConversationExchangeAppended(IChatPageRepository chatPageRepository)
    : INotificationHandler<DomainEventNotification<ConversationExchangeAppended>>
{
    public async Task Handle(DomainEventNotification<ConversationExchangeAppended> n, CancellationToken ct)
    {
        var entry = await chatPageRepository.FindByBotKey(n.DomainEvent.BotKey.Value, ct);
        if (entry is null)
        {
            return;
        }

        entry.AppendExchange(
            n.DomainEvent.Id.Value,
            n.DomainEvent.Exchange.Question,
            n.DomainEvent.Exchange.Answer,
            n.DomainEvent.Exchange.Citation,
            n.DomainEvent.Exchange.MoreExplanation,
            n.DomainEvent.Exchange.SocraticVariant);
        await chatPageRepository.Save(entry, ct);
    }
}
