using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotDetail.Application.Handlers;

public sealed class ChatbotDetailUpdatedOnChatbotUnarchived(IChatbotDetailRepository chatbotDetailRepository)
    : DomainEventHandler<ChatbotUnarchived>
{
    protected override async Task Handle(ChatbotUnarchived chatbotUnarchived, CancellationToken cancellationToken)
    {
        Domain.ChatbotDetail? chatbotDetail = await chatbotDetailRepository.FindById(chatbotUnarchived.Id.Value, cancellationToken);
        if (chatbotDetail is null)
        {
            return;
        }

        chatbotDetail.MarkUnarchived();
        await chatbotDetailRepository.Update(chatbotDetail, cancellationToken);
    }
}
