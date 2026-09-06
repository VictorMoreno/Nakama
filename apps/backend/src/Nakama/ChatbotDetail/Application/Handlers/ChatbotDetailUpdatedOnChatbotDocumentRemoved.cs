using MediatR;
using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotDetail.Application.Handlers;

using ChatbotDetail = Nakama.ChatbotDetail.Domain.ChatbotDetail;

/// <summary>Updates <see cref="ChatbotDetail"/> from <see cref="ChatbotDocumentRemoved"/>.</summary>
public sealed class ChatbotDetailUpdatedOnChatbotDocumentRemoved(IChatbotDetailRepository chatbotDetailRepository)
    : INotificationHandler<DomainEventNotification<ChatbotDocumentRemoved>>
{
    public async Task Handle(DomainEventNotification<ChatbotDocumentRemoved> n, CancellationToken ct)
    {
        var entry = await chatbotDetailRepository.FindByKey(n.DomainEvent.Key.Value, ct);
        if (entry is null)
        {
            return;
        }

        entry.RemoveDocument(n.DomainEvent.Index);
        await chatbotDetailRepository.Save(entry, ct);
    }
}
