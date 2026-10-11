using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotSummaries.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotSummaries.Application.Handlers;

public sealed class ChatbotSummaryUpdatedOnChatbotDocumentAdded(IChatbotSummaryRepository chatbotSummaryRepository)
    : DomainEventHandler<ChatbotDocumentAdded>
{
    protected override async Task Handle(ChatbotDocumentAdded chatbotDocumentAdded, CancellationToken cancellationToken)
    {
        ChatbotSummary? chatbotSummary = await chatbotSummaryRepository.FindById(chatbotDocumentAdded.Id.Value, cancellationToken);

        if (chatbotSummary is null)
        {
            return;
        }

        chatbotSummary.IncrementDocsCount();
        await chatbotSummaryRepository.Update(chatbotSummary, cancellationToken);
    }
}
