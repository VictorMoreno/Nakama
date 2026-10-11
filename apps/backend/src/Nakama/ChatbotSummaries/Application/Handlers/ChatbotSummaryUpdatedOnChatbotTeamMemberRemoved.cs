using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotSummaries.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotSummaries.Application.Handlers;

public sealed class ChatbotSummaryUpdatedOnChatbotTeamMemberRemoved(IChatbotSummaryRepository chatbotSummaryRepository)
    : DomainEventHandler<ChatbotTeamMemberRemoved>
{
    protected override async Task Handle(ChatbotTeamMemberRemoved chatbotTeamMemberRemoved, CancellationToken cancellationToken)
    {
        ChatbotSummary? chatbotSummary = await chatbotSummaryRepository.FindById(chatbotTeamMemberRemoved.Id.Value, cancellationToken);

        if (chatbotSummary is null)
        {
            return;
        }

        chatbotSummary.DecrementTeamCount();
        await chatbotSummaryRepository.Update(chatbotSummary, cancellationToken);
    }
}
