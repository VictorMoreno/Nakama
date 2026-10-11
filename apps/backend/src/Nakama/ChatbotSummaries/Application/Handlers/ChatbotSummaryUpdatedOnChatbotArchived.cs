using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotSummaries.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotSummaries.Application.Handlers;

public sealed class ChatbotSummaryUpdatedOnChatbotArchived(IChatbotSummaryRepository chatbotSummaryRepository)
    : DomainEventHandler<ChatbotArchived>
{
    protected override async Task Handle(ChatbotArchived chatbotArchived, CancellationToken cancellationToken)
    {
        ChatbotSummary? chatbotSummary = await chatbotSummaryRepository.FindById(chatbotArchived.Id.Value, cancellationToken);

        if (chatbotSummary is null)
        {
            return;
        }

        chatbotSummary.MarkArchived();
        await chatbotSummaryRepository.Update(chatbotSummary, cancellationToken);
    }
}
