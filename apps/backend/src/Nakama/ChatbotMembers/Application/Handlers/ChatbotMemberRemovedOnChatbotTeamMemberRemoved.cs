using Nakama.ChatbotMembers.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotMembers.Application.Handlers;

public sealed class ChatbotMemberRemovedOnChatbotTeamMemberRemoved(IChatbotMemberRepository chatbotMemberRepository)
    : DomainEventHandler<ChatbotTeamMemberRemoved>
{
    protected override async Task Handle(ChatbotTeamMemberRemoved chatbotTeamMemberRemoved, CancellationToken cancellationToken)
    {
        IReadOnlyList<ChatbotMember> members = await chatbotMemberRepository.Search(
            new ChatbotMemberByChatbotAndUserSpecification(chatbotTeamMemberRemoved.Id.Value, chatbotTeamMemberRemoved.UserId),
            cancellationToken);

        ChatbotMember? member = members.FirstOrDefault();
        if (member is null)
        {
            return;
        }

        await chatbotMemberRepository.Remove(member, cancellationToken);
    }
}
