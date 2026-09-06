using MediatR;
using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotsList.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotsList.Application.Handlers;

/// <summary>Updates <see cref="ChatbotSummary"/> from <see cref="ChatbotIconUpdated"/>.</summary>
public sealed class ChatbotSummaryUpdatedOnChatbotIconUpdated(IChatbotSummaryRepository chatbotSummaryRepository)
    : INotificationHandler<DomainEventNotification<ChatbotIconUpdated>>
{
    public async Task Handle(DomainEventNotification<ChatbotIconUpdated> n, CancellationToken ct)
    {
        var entry = await chatbotSummaryRepository.FindByKey(n.DomainEvent.Key.Value, ct);
        // Event arrived before ChatbotCreated - shouldn't happen in single-process sync
        // dispatch, but not assumed (same rationale as the original raw-session projection).
        if (entry is null)
        {
            return;
        }

        entry.UpdateIcon(n.DomainEvent.IconUrl);
        await chatbotSummaryRepository.Save(entry, ct);
    }
}
