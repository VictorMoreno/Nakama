using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotsList.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotsList.Application.Handlers;

public sealed class ChatbotSummaryUpdatedOnChatbotArchived(IChatbotSummaryRepository chatbotSummaryRepository)
    : DomainEventHandler<ChatbotArchived>
{
    protected override async Task Handle(ChatbotArchived chatbotArchived, CancellationToken ct)
    {
        var chatbotSummary = await chatbotSummaryRepository.FindById(chatbotArchived.Id.Value, ct);
        // Event arrived before ChatbotCreated - shouldn't happen in single-process sync
        // dispatch, but not assumed (same rationale as the original raw-session projection).
        if (chatbotSummary is null)
        {
            return;
        }

        chatbotSummary.MarkArchived();
        await chatbotSummaryRepository.Update(chatbotSummary, ct);
    }
}
