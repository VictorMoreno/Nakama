using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotDetail.Application.Handlers;

public sealed class ChatbotDetailUpdatedOnChatbotDocumentRemoved(IChatbotDetailRepository chatbotDetailRepository)
    : DomainEventHandler<ChatbotDocumentRemoved>
{
    protected override async Task Handle(ChatbotDocumentRemoved chatbotDocumentRemoved, CancellationToken ct)
    {
        var chatbotDetail = await chatbotDetailRepository.FindById(chatbotDocumentRemoved.Id.Value, ct);
        if (chatbotDetail is null)
        {
            return;
        }

        chatbotDetail.RemoveDocument(chatbotDocumentRemoved.Index);
        await chatbotDetailRepository.Update(chatbotDetail, ct);
    }
}
