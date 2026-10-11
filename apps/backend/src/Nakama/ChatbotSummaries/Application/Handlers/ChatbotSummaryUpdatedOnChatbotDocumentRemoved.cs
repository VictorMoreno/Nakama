using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotSummaries.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotSummaries.Application.Handlers;

public sealed class ChatbotSummaryUpdatedOnChatbotDocumentRemoved(IChatbotSummaryRepository chatbotSummaryRepository)
    : DomainEventHandler<ChatbotDocumentRemoved>
{
    protected override async Task Handle(ChatbotDocumentRemoved chatbotDocumentRemoved, CancellationToken cancellationToken)
    {
        ChatbotSummary? chatbotSummary = await chatbotSummaryRepository.FindById(chatbotDocumentRemoved.Id.Value, cancellationToken);

        if (chatbotSummary is null)
        {
            return;
        }

        chatbotSummary.DecrementDocsCount();
        await chatbotSummaryRepository.Update(chatbotSummary, cancellationToken);
    }
}
