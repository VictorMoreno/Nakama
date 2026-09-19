using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotsList.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotsList.Application.Handlers;

public sealed class ChatbotSummaryUpdatedOnChatbotTeamMemberRemoved(IChatbotSummaryRepository chatbotSummaryRepository)
    : DomainEventHandler<ChatbotTeamMemberRemoved>
{
    protected override async Task Handle(ChatbotTeamMemberRemoved chatbotTeamMemberRemoved, CancellationToken ct)
    {
        var chatbotSummary = await chatbotSummaryRepository.FindById(chatbotTeamMemberRemoved.Id.Value, ct);
        // Event arrived before ChatbotCreated - shouldn't happen in single-process sync
        // dispatch, but not assumed (same rationale as the original raw-session projection).
        if (chatbotSummary is null)
        {
            return;
        }

        chatbotSummary.DecrementTeamCount();
        await chatbotSummaryRepository.Update(chatbotSummary, ct);
    }
}
