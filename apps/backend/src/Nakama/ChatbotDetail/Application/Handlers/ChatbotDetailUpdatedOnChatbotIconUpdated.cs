using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotDetail.Application.Handlers;

using ChatbotDetail = Nakama.ChatbotDetail.Domain.ChatbotDetail;

public sealed class ChatbotDetailUpdatedOnChatbotIconUpdated(IChatbotDetailRepository chatbotDetailRepository)
    : DomainEventHandler<ChatbotIconUpdated>
{
    protected override async Task Handle(ChatbotIconUpdated chatbotIconUpdated, CancellationToken ct)
    {
        var chatbotDetail = await chatbotDetailRepository.FindById(chatbotIconUpdated.Id.Value, ct);
        if (chatbotDetail is null)
        {
            return;
        }

        chatbotDetail.UpdateIcon(chatbotIconUpdated.IconUrl);
        await chatbotDetailRepository.Update(chatbotDetail, ct);
    }
}
