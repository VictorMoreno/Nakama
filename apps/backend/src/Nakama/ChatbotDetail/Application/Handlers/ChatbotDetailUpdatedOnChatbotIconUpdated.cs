using MediatR;
using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotDetail.Application.Handlers;

using ChatbotDetail = Nakama.ChatbotDetail.Domain.ChatbotDetail;

/// <summary>Updates <see cref="ChatbotDetail"/> from <see cref="ChatbotIconUpdated"/>.</summary>
public sealed class ChatbotDetailUpdatedOnChatbotIconUpdated(IChatbotDetailRepository chatbotDetailRepository)
    : INotificationHandler<DomainEventNotification<ChatbotIconUpdated>>
{
    public async Task Handle(DomainEventNotification<ChatbotIconUpdated> n, CancellationToken ct)
    {
        var entry = await chatbotDetailRepository.FindByKey(n.DomainEvent.Key.Value, ct);
        if (entry is null)
        {
            return;
        }

        entry.UpdateIcon(n.DomainEvent.IconUrl);
        await chatbotDetailRepository.Save(entry, ct);
    }
}
