using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotSummaries.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotSummaries.Application.Handlers;

public sealed class ChatbotSummaryUpdatedOnChatbotUnarchived(IChatbotSummaryRepository chatbotSummaryRepository)
    : DomainEventHandler<ChatbotUnarchived>
{
    protected override async Task Handle(ChatbotUnarchived chatbotUnarchived, CancellationToken cancellationToken)
    {
        ChatbotSummary? chatbotSummary = await chatbotSummaryRepository.FindById(chatbotUnarchived.Id.Value, cancellationToken);

        if (chatbotSummary is null)
        {
            return;
        }

        chatbotSummary.MarkUnarchived();
        await chatbotSummaryRepository.Update(chatbotSummary, cancellationToken);
    }
}
