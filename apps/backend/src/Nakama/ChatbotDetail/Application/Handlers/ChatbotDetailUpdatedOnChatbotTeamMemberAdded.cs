using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotDetail.Application.Handlers;

public sealed class ChatbotDetailUpdatedOnChatbotTeamMemberAdded(IChatbotDetailRepository chatbotDetailRepository)
    : DomainEventHandler<ChatbotTeamMemberAdded>
{
    protected override async Task Handle(ChatbotTeamMemberAdded chatbotTeamMemberAdded, CancellationToken cancellationToken)
    {
        Domain.ChatbotDetail? chatbotDetail = await chatbotDetailRepository.FindById(chatbotTeamMemberAdded.Id.Value, cancellationToken);
        if (chatbotDetail is null)
        {
            return;
        }

        chatbotDetail.AddTeamMember(chatbotTeamMemberAdded.Member.UserId, chatbotTeamMemberAdded.Member.Role.ToString());
        await chatbotDetailRepository.Update(chatbotDetail, cancellationToken);
    }
}
