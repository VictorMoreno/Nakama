using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotSummaries.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotSummaries.Application.Handlers;

public sealed class ChatbotSummaryUpdatedOnChatbotTeamMemberRemoved(IChatbotSummaryRepository chatbotSummaryRepository)
    : DomainEventHandler<ChatbotTeamMemberRemoved>
{
    protected override async Task Handle(ChatbotTeamMemberRemoved chatbotTeamMemberRemoved, CancellationToken cancellationToken)
    {
        var chatbotSummary = await chatbotSummaryRepository.FindById(chatbotTeamMemberRemoved.Id.Value, cancellationToken);
        // Event arrived before ChatbotCreated - shouldn't happen in single-process sync
        // dispatch, but not assumed (same rationale as the original raw-session projection).
        if (chatbotSummary is null)
        {
            return;
        }

        chatbotSummary.DecrementTeamCount();
        await chatbotSummaryRepository.Update(chatbotSummary, cancellationToken);
    }
}
