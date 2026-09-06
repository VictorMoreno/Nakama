using MediatR;
using Nakama.Chatbots.Domain.Events;
using Nakama.FormationDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationDetail.Application.Handlers;

/// <summary>
/// Keeps this module's local Chatbot-label lookup current - see <see cref="ChatbotLabelRef"/>.
/// Listens to <see cref="ChatbotCreated"/>, a totally different module's <c>Domain/Events</c> -
/// the intentional projection coupling the plan calls out - so <see cref="FormationDetail"/> can
/// resolve the denormalized <c>LinkedChatbotLabel</c> caption without reaching into the
/// <c>ChatbotsList</c> read model.
/// </summary>
public sealed class ChatbotLabelRefCreatedOnChatbotCreated(IChatbotLabelRefRepository chatbotLabelRefRepository)
    : INotificationHandler<DomainEventNotification<ChatbotCreated>>
{
    public Task Handle(DomainEventNotification<ChatbotCreated> n, CancellationToken ct) =>
        chatbotLabelRefRepository.Save(ChatbotLabelRef.Create(n.DomainEvent.Key.Value, n.DomainEvent.Label), ct);
}
