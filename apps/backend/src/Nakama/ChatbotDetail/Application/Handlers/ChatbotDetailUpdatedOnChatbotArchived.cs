using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotDetail.Application.Handlers;

public sealed class ChatbotDetailUpdatedOnChatbotArchived(IChatbotDetailRepository chatbotDetailRepository)
    : DomainEventHandler<ChatbotArchived>
{
    protected override async Task Handle(ChatbotArchived chatbotArchived, CancellationToken cancellationToken)
    {
        var chatbotDetail = await chatbotDetailRepository.FindById(chatbotArchived.Id.Value, cancellationToken);
        if (chatbotDetail is null)
        {
            return;
        }

        chatbotDetail.MarkArchived();
        await chatbotDetailRepository.Update(chatbotDetail, cancellationToken);
    }
}
