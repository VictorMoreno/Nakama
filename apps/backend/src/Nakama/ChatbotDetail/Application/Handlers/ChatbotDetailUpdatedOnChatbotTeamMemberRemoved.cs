using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotDetail.Application.Handlers;

public sealed class ChatbotDetailUpdatedOnChatbotTeamMemberRemoved(IChatbotDetailRepository chatbotDetailRepository)
    : DomainEventHandler<ChatbotTeamMemberRemoved>
{
    protected override async Task Handle(ChatbotTeamMemberRemoved chatbotTeamMemberRemoved, CancellationToken cancellationToken)
    {
        Domain.ChatbotDetail? chatbotDetail = await chatbotDetailRepository.FindById(chatbotTeamMemberRemoved.Id.Value, cancellationToken);
        if (chatbotDetail is null)
        {
            return;
        }

        chatbotDetail.RemoveTeamMember(chatbotTeamMemberRemoved.UserId);
        await chatbotDetailRepository.Update(chatbotDetail, cancellationToken);
    }
}
