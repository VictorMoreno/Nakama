using MediatR;
using Nakama.ChatPage.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatPage.Application.Handlers;

using ChatPage = Nakama.ChatPage.Domain.ChatPage;

/// <summary>Creates <see cref="ChatPage"/>, the read entity behind the <c>/chat</c> route.</summary>
public sealed class ChatPageCreatedOnChatbotCreated(IChatPageRepository chatPageRepository)
    : INotificationHandler<DomainEventNotification<ChatbotCreated>>
{
    public Task Handle(DomainEventNotification<ChatbotCreated> n, CancellationToken ct) =>
        chatPageRepository.Save(ChatPage.Create(n.DomainEvent.Key.Value, n.DomainEvent.Label), ct);
}
