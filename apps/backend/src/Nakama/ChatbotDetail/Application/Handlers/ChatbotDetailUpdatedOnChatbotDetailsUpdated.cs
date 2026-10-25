using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotDetail.Application.Handlers;

public sealed class ChatbotDetailUpdatedOnChatbotDetailsUpdated(IChatbotDetailRepository chatbotDetailRepository)
    : DomainEventHandler<ChatbotDetailsUpdated>
{
    protected override async Task Handle(ChatbotDetailsUpdated chatbotDetailsUpdated, CancellationToken cancellationToken)
    {
        Domain.ChatbotDetail? chatbotDetail = await chatbotDetailRepository.FindById(chatbotDetailsUpdated.Id.Value, cancellationToken);
        if (chatbotDetail is null)
        {
            return;
        }

        chatbotDetail.UpdateDetails(chatbotDetailsUpdated.Label, chatbotDetailsUpdated.Description);
        await chatbotDetailRepository.Update(chatbotDetail, cancellationToken);
    }
}
