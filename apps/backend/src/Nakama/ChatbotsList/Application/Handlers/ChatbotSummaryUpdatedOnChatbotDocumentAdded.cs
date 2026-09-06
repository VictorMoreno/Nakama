using MediatR;
using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotsList.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotsList.Application.Handlers;

/// <summary>Updates <see cref="ChatbotSummary"/> from <see cref="ChatbotDocumentAdded"/>.</summary>
public sealed class ChatbotSummaryUpdatedOnChatbotDocumentAdded(IChatbotSummaryRepository chatbotSummaryRepository)
    : INotificationHandler<DomainEventNotification<ChatbotDocumentAdded>>
{
    public async Task Handle(DomainEventNotification<ChatbotDocumentAdded> n, CancellationToken ct)
    {
        var entry = await chatbotSummaryRepository.FindByKey(n.DomainEvent.Key.Value, ct);
        // Event arrived before ChatbotCreated - shouldn't happen in single-process sync
        // dispatch, but not assumed (same rationale as the original raw-session projection).
        if (entry is null)
        {
            return;
        }

        entry.IncrementDocsCount();
        await chatbotSummaryRepository.Save(entry, ct);
    }
}
