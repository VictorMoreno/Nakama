using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotSummaries.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotSummaries.Application.Handlers;

public sealed class ChatbotSummaryUpdatedOnChatbotIconUpdated(IChatbotSummaryRepository chatbotSummaryRepository)
    : DomainEventHandler<ChatbotIconUpdated>
{
    protected override async Task Handle(ChatbotIconUpdated chatbotIconUpdated, CancellationToken cancellationToken)
    {
        ChatbotSummary? chatbotSummary = await chatbotSummaryRepository.FindById(chatbotIconUpdated.Id.Value, cancellationToken);

        if (chatbotSummary is null)
        {
            return;
        }

        chatbotSummary.UpdateIcon(chatbotIconUpdated.IconUrl);
        await chatbotSummaryRepository.Update(chatbotSummary, cancellationToken);
    }
}
