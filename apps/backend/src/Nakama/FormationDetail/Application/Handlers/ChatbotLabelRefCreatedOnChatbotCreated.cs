using Nakama.Chatbots.Domain.Events;
using Nakama.FormationDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationDetail.Application.Handlers;

public sealed class ChatbotLabelRefCreatedOnChatbotCreated(IChatbotLabelRefRepository chatbotLabelRefRepository)
    : DomainEventHandler<ChatbotCreated>
{
    protected override Task Handle(ChatbotCreated chatbotCreated, CancellationToken ct) =>
        chatbotLabelRefRepository.Save(ChatbotLabelRef.Create(chatbotCreated.Id.Value, chatbotCreated.Label), ct);
}
