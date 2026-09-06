using MediatR;
using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotsList.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotsList.Application.Handlers;

/// <summary>Updates <see cref="ChatbotSummary"/> from <see cref="ChatbotDocumentRemoved"/>.</summary>
public sealed class ChatbotSummaryUpdatedOnChatbotDocumentRemoved(IChatbotSummaryRepository chatbotSummaryRepository)
    : INotificationHandler<DomainEventNotification<ChatbotDocumentRemoved>>
{
    public async Task Handle(DomainEventNotification<ChatbotDocumentRemoved> n, CancellationToken ct)
    {
        var entry = await chatbotSummaryRepository.FindByKey(n.DomainEvent.Key.Value, ct);
        // Event arrived before ChatbotCreated - shouldn't happen in single-process sync
        // dispatch, but not assumed (same rationale as the original raw-session projection).
        if (entry is null)
        {
            return;
        }

        entry.DecrementDocsCount();
        await chatbotSummaryRepository.Save(entry, ct);
    }
}
