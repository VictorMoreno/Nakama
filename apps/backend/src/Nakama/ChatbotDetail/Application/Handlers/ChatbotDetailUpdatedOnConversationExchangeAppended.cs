using MediatR;
using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotDetail.Application.Handlers;

using ChatbotDetail = Nakama.ChatbotDetail.Domain.ChatbotDetail;

/// <summary>Updates <see cref="ChatbotDetail"/> from <see cref="ConversationExchangeAppended"/>.</summary>
public sealed class ChatbotDetailUpdatedOnConversationExchangeAppended(IChatbotDetailRepository chatbotDetailRepository)
    : INotificationHandler<DomainEventNotification<ConversationExchangeAppended>>
{
    public async Task Handle(DomainEventNotification<ConversationExchangeAppended> n, CancellationToken ct)
    {
        var entry = await chatbotDetailRepository.FindByKey(n.DomainEvent.BotKey.Value, ct);
        if (entry is null)
        {
            return;
        }

        entry.RecordQuestion(n.DomainEvent.Exchange.Question);
        await chatbotDetailRepository.Save(entry, ct);
    }
}
