using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotsList.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotsList.Application.Handlers;

public sealed class ChatbotSummaryUpdatedOnChatbotTeamMemberAdded(IChatbotSummaryRepository chatbotSummaryRepository)
    : DomainEventHandler<ChatbotTeamMemberAdded>
{
    protected override async Task Handle(ChatbotTeamMemberAdded chatbotTeamMemberAdded, CancellationToken ct)
    {
        var chatbotSummary = await chatbotSummaryRepository.FindById(chatbotTeamMemberAdded.Id.Value, ct);
        // Event arrived before ChatbotCreated - shouldn't happen in single-process sync
        // dispatch, but not assumed (same rationale as the original raw-session projection).
        if (chatbotSummary is null)
        {
            return;
        }

        chatbotSummary.IncrementTeamCount();
        await chatbotSummaryRepository.Update(chatbotSummary, ct);
    }
}
