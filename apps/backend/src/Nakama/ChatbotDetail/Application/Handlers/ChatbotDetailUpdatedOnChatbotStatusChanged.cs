using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotDetail.Application.Handlers;

public sealed class ChatbotDetailUpdatedOnChatbotStatusChanged(IChatbotDetailRepository chatbotDetailRepository)
    : DomainEventHandler<ChatbotStatusChanged>
{
    protected override async Task Handle(ChatbotStatusChanged chatbotStatusChanged, CancellationToken cancellationToken)
    {
        var chatbotDetail = await chatbotDetailRepository.FindById(chatbotStatusChanged.Id.Value, cancellationToken);
        if (chatbotDetail is null)
        {
            return;
        }

        chatbotDetail.UpdateStatus(chatbotStatusChanged.Status.ToString());
        await chatbotDetailRepository.Update(chatbotDetail, cancellationToken);
    }
}
