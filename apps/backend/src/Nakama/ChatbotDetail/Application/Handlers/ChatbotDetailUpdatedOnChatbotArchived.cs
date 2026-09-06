using MediatR;
using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotDetail.Application.Handlers;

using ChatbotDetail = Nakama.ChatbotDetail.Domain.ChatbotDetail;

/// <summary>Updates <see cref="ChatbotDetail"/> from <see cref="ChatbotArchived"/>.</summary>
public sealed class ChatbotDetailUpdatedOnChatbotArchived(IChatbotDetailRepository chatbotDetailRepository)
    : INotificationHandler<DomainEventNotification<ChatbotArchived>>
{
    public async Task Handle(DomainEventNotification<ChatbotArchived> n, CancellationToken ct)
    {
        var entry = await chatbotDetailRepository.FindByKey(n.DomainEvent.Key.Value, ct);
        if (entry is null)
        {
            return;
        }

        entry.MarkArchived();
        await chatbotDetailRepository.Save(entry, ct);
    }
}
