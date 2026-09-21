using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotSummaries.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotSummaries.Application.Handlers;

public sealed class ChatbotSummaryCreatedOnChatbotCreated(IChatbotSummaryRepository chatbotSummaryRepository)
    : DomainEventHandler<ChatbotCreated>
{
    protected override Task Handle(ChatbotCreated chatbotCreated, CancellationToken ct) =>
        chatbotSummaryRepository.Save(
            ChatbotSummary.Create(
                chatbotCreated.Id.Value,
                chatbotCreated.Label,
                chatbotCreated.Description,
                chatbotCreated.Status.ToString(),
                chatbotCreated.General),
            ct);
}
