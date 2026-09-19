using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotsList.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotsList.Application.Handlers;

public sealed class ChatbotSummaryUpdatedOnChatbotDocumentAdded(IChatbotSummaryRepository chatbotSummaryRepository)
    : DomainEventHandler<ChatbotDocumentAdded>
{
    protected override async Task Handle(ChatbotDocumentAdded chatbotDocumentAdded, CancellationToken ct)
    {
        var chatbotSummary = await chatbotSummaryRepository.FindById(chatbotDocumentAdded.Id.Value, ct);
        // Event arrived before ChatbotCreated - shouldn't happen in single-process sync
        // dispatch, but not assumed (same rationale as the original raw-session projection).
        if (chatbotSummary is null)
        {
            return;
        }

        chatbotSummary.IncrementDocsCount();
        await chatbotSummaryRepository.Update(chatbotSummary, ct);
    }
}
