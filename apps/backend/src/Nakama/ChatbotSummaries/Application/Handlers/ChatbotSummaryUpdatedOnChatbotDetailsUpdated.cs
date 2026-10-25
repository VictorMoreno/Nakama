using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotSummaries.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotSummaries.Application.Handlers;

public sealed class ChatbotSummaryUpdatedOnChatbotDetailsUpdated(IChatbotSummaryRepository chatbotSummaryRepository)
    : DomainEventHandler<ChatbotDetailsUpdated>
{
    protected override async Task Handle(ChatbotDetailsUpdated chatbotDetailsUpdated, CancellationToken cancellationToken)
    {
        ChatbotSummary? chatbotSummary = await chatbotSummaryRepository.FindById(chatbotDetailsUpdated.Id.Value, cancellationToken);

        if (chatbotSummary is null)
        {
            return;
        }

        chatbotSummary.UpdateDetails(chatbotDetailsUpdated.Label, chatbotDetailsUpdated.Description);
        await chatbotSummaryRepository.Update(chatbotSummary, cancellationToken);
    }
}
