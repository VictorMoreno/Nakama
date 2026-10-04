using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotSummaries.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotSummaries.Application.Handlers;

public sealed class ChatbotSummaryUpdatedOnChatbotTeamMemberAdded(IChatbotSummaryRepository chatbotSummaryRepository)
    : DomainEventHandler<ChatbotTeamMemberAdded>
{
    protected override async Task Handle(ChatbotTeamMemberAdded chatbotTeamMemberAdded, CancellationToken cancellationToken)
    {
        var chatbotSummary = await chatbotSummaryRepository.FindById(chatbotTeamMemberAdded.Id.Value, cancellationToken);
        // Event arrived before ChatbotCreated - shouldn't happen in single-process sync
        // dispatch, but not assumed (same rationale as the original raw-session projection).
        if (chatbotSummary is null)
        {
            return;
        }

        chatbotSummary.IncrementTeamCount();
        await chatbotSummaryRepository.Update(chatbotSummary, cancellationToken);
    }
}
