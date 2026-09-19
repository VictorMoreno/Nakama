using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotsList.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotsList.Application.Handlers;

public sealed class ChatbotSummaryUpdatedOnChatbotIconUpdated(IChatbotSummaryRepository chatbotSummaryRepository)
    : DomainEventHandler<ChatbotIconUpdated>
{
    protected override async Task Handle(ChatbotIconUpdated chatbotIconUpdated, CancellationToken ct)
    {
        var chatbotSummary = await chatbotSummaryRepository.FindById(chatbotIconUpdated.Id.Value, ct);
        // Event arrived before ChatbotCreated - shouldn't happen in single-process sync
        // dispatch, but not assumed (same rationale as the original raw-session projection).
        if (chatbotSummary is null)
        {
            return;
        }

        chatbotSummary.UpdateIcon(chatbotIconUpdated.IconUrl);
        await chatbotSummaryRepository.Update(chatbotSummary, ct);
    }
}
