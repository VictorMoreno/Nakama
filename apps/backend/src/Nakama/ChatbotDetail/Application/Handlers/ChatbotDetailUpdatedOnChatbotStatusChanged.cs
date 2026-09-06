using MediatR;
using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotDetail.Application.Handlers;

using ChatbotDetail = Nakama.ChatbotDetail.Domain.ChatbotDetail;

/// <summary>Updates <see cref="ChatbotDetail"/> from <see cref="ChatbotStatusChanged"/>.</summary>
public sealed class ChatbotDetailUpdatedOnChatbotStatusChanged(IChatbotDetailRepository chatbotDetailRepository)
    : INotificationHandler<DomainEventNotification<ChatbotStatusChanged>>
{
    public async Task Handle(DomainEventNotification<ChatbotStatusChanged> n, CancellationToken ct)
    {
        var entry = await chatbotDetailRepository.FindByKey(n.DomainEvent.Key.Value, ct);
        if (entry is null)
        {
            return;
        }

        entry.UpdateStatus(n.DomainEvent.Status.ToString());
        await chatbotDetailRepository.Save(entry, ct);
    }
}
