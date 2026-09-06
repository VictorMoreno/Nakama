using MediatR;
using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotDetail.Application.Handlers;

using ChatbotDetail = Nakama.ChatbotDetail.Domain.ChatbotDetail;

/// <summary>Creates <see cref="ChatbotDetail"/>, the read entity behind all 4 tabs of the <c>/chatbots/:key</c> route.</summary>
public sealed class ChatbotDetailCreatedOnChatbotCreated(IChatbotDetailRepository chatbotDetailRepository)
    : INotificationHandler<DomainEventNotification<ChatbotCreated>>
{
    public Task Handle(DomainEventNotification<ChatbotCreated> n, CancellationToken ct) =>
        chatbotDetailRepository.Save(
            ChatbotDetail.Create(
                n.DomainEvent.Key.Value,
                n.DomainEvent.Label,
                n.DomainEvent.Description,
                n.DomainEvent.Status.ToString(),
                n.DomainEvent.General),
            ct);
}
