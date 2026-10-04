using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotSummaries.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotSummaries.Application.Handlers;

public sealed class ChatbotSummaryUpdatedOnChatbotDocumentRemoved(IChatbotSummaryRepository chatbotSummaryRepository)
    : DomainEventHandler<ChatbotDocumentRemoved>
{
    protected override async Task Handle(ChatbotDocumentRemoved chatbotDocumentRemoved, CancellationToken cancellationToken)
    {
        var chatbotSummary = await chatbotSummaryRepository.FindById(chatbotDocumentRemoved.Id.Value, cancellationToken);
        // Event arrived before ChatbotCreated - shouldn't happen in single-process sync
        // dispatch, but not assumed (same rationale as the original raw-session projection).
        if (chatbotSummary is null)
        {
            return;
        }

        chatbotSummary.DecrementDocsCount();
        await chatbotSummaryRepository.Update(chatbotSummary, cancellationToken);
    }
}
