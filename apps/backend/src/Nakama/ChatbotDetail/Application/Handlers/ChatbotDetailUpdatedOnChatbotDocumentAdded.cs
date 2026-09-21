using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotDetail.Application.Handlers;

public sealed class ChatbotDetailUpdatedOnChatbotDocumentAdded(IChatbotDetailRepository chatbotDetailRepository)
    : DomainEventHandler<ChatbotDocumentAdded>
{
    protected override async Task Handle(ChatbotDocumentAdded chatbotDocumentAdded, CancellationToken ct)
    {
        var chatbotDetail = await chatbotDetailRepository.FindById(chatbotDocumentAdded.Id.Value, ct);
        if (chatbotDetail is null)
        {
            return;
        }

        chatbotDetail.AddDocument(
            chatbotDocumentAdded.Document.Id,
            chatbotDocumentAdded.Document.Name,
            chatbotDocumentAdded.Document.Origin,
            chatbotDocumentAdded.Document.Status.ToString(),
            chatbotDocumentAdded.Document.StatusLabel,
            chatbotDocumentAdded.Document.Updated,
            chatbotDocumentAdded.Document.FileName);
        await chatbotDetailRepository.Update(chatbotDetail, ct);
    }
}
