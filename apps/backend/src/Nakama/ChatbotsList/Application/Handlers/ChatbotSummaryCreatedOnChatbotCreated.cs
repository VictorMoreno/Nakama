using MediatR;
using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotsList.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotsList.Application.Handlers;

/// <summary>Creates <see cref="ChatbotSummary"/>, the read entity behind the <c>/chatbots</c> listing route.</summary>
public sealed class ChatbotSummaryCreatedOnChatbotCreated(IChatbotSummaryRepository chatbotSummaryRepository)
    : INotificationHandler<DomainEventNotification<ChatbotCreated>>
{
    public Task Handle(DomainEventNotification<ChatbotCreated> n, CancellationToken ct) =>
        chatbotSummaryRepository.Save(
            ChatbotSummary.Create(
                n.DomainEvent.Key.Value,
                n.DomainEvent.Label,
                n.DomainEvent.Description,
                n.DomainEvent.Status.ToString(),
                n.DomainEvent.General),
            ct);
}
