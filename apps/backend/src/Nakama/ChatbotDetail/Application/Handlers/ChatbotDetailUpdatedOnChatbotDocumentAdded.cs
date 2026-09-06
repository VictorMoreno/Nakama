using MediatR;
using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotDetail.Application.Handlers;

using ChatbotDetail = Nakama.ChatbotDetail.Domain.ChatbotDetail;

/// <summary>Updates <see cref="ChatbotDetail"/> from <see cref="ChatbotDocumentAdded"/>.</summary>
public sealed class ChatbotDetailUpdatedOnChatbotDocumentAdded(IChatbotDetailRepository chatbotDetailRepository)
    : INotificationHandler<DomainEventNotification<ChatbotDocumentAdded>>
{
    public async Task Handle(DomainEventNotification<ChatbotDocumentAdded> n, CancellationToken ct)
    {
        var entry = await chatbotDetailRepository.FindByKey(n.DomainEvent.Key.Value, ct);
        if (entry is null)
        {
            return;
        }

        entry.AddDocument(
            n.DomainEvent.Document.Name,
            n.DomainEvent.Document.Origin,
            n.DomainEvent.Document.Status.ToString(),
            n.DomainEvent.Document.StatusLabel,
            n.DomainEvent.Document.Updated);
        await chatbotDetailRepository.Save(entry, ct);
    }
}
