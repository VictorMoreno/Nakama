using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotDetail.Application.Handlers;

public sealed class ChatbotDetailUpdatedOnChatbotTeamMemberRemoved(IChatbotDetailRepository chatbotDetailRepository)
    : DomainEventHandler<ChatbotTeamMemberRemoved>
{
    protected override async Task Handle(ChatbotTeamMemberRemoved chatbotTeamMemberRemoved, CancellationToken ct)
    {
        var chatbotDetail = await chatbotDetailRepository.FindById(chatbotTeamMemberRemoved.Id.Value, ct);
        if (chatbotDetail is null)
        {
            return;
        }

        chatbotDetail.RemoveTeamMember(chatbotTeamMemberRemoved.Index);
        await chatbotDetailRepository.Update(chatbotDetail, ct);
    }
}
