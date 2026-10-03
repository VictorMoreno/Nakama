using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotDetail.Application.Handlers;

using ChatbotDetail = Nakama.ChatbotDetail.Domain.ChatbotDetail;

public sealed class ChatbotDetailCreatedOnChatbotCreated(IChatbotDetailRepository chatbotDetailRepository)
    : DomainEventHandler<ChatbotCreated>
{
    protected override Task Handle(ChatbotCreated chatbotCreated, CancellationToken ct) =>
        chatbotDetailRepository.Save(
            ChatbotDetail.Create(
                chatbotCreated.Id.Value,
                chatbotCreated.Label,
                chatbotCreated.Description,
                chatbotCreated.Status.ToString(),
                chatbotCreated.General,
                chatbotCreated.Settings.Socratic,
                chatbotCreated.Settings.ExplicaMas,
                chatbotCreated.Settings.Provider.ToString(),
                chatbotCreated.Settings.Model),
            ct);
}
