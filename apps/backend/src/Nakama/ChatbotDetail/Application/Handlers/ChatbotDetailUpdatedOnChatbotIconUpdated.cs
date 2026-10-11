using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotDetail.Application.Handlers;

public sealed class ChatbotDetailUpdatedOnChatbotIconUpdated(IChatbotDetailRepository chatbotDetailRepository)
    : DomainEventHandler<ChatbotIconUpdated>
{
    protected override async Task Handle(ChatbotIconUpdated chatbotIconUpdated, CancellationToken cancellationToken)
    {
        Domain.ChatbotDetail? chatbotDetail = await chatbotDetailRepository.FindById(chatbotIconUpdated.Id.Value, cancellationToken);
        if (chatbotDetail is null)
        {
            return;
        }

        chatbotDetail.UpdateIcon(chatbotIconUpdated.IconUrl);
        await chatbotDetailRepository.Update(chatbotDetail, cancellationToken);
    }
}
