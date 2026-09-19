using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotsList.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotsList.Application.Handlers;

public sealed class ChatbotSummaryUpdatedOnChatbotStatusChanged(IChatbotSummaryRepository chatbotSummaryRepository)
    : DomainEventHandler<ChatbotStatusChanged>
{
    protected override async Task Handle(ChatbotStatusChanged chatbotStatusChanged, CancellationToken ct)
    {
        var chatbotSummary = await chatbotSummaryRepository.FindById(chatbotStatusChanged.Id.Value, ct);
        // Event arrived before ChatbotCreated - shouldn't happen in single-process sync
        // dispatch, but not assumed (same rationale as the original raw-session projection).
        if (chatbotSummary is null)
        {
            return;
        }

        chatbotSummary.UpdateStatus(chatbotStatusChanged.Status.ToString());
        await chatbotSummaryRepository.Update(chatbotSummary, ct);
    }
}
