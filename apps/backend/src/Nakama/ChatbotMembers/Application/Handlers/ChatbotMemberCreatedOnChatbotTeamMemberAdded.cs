using Nakama.ChatbotMembers.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotMembers.Application.Handlers;

public sealed class ChatbotMemberCreatedOnChatbotTeamMemberAdded(IChatbotMemberRepository chatbotMemberRepository)
    : DomainEventHandler<ChatbotTeamMemberAdded>
{
    protected override Task Handle(ChatbotTeamMemberAdded chatbotTeamMemberAdded, CancellationToken cancellationToken) =>
        chatbotMemberRepository.Save(
            ChatbotMember.Create(Guid.NewGuid(), chatbotTeamMemberAdded.Id.Value, chatbotTeamMemberAdded.Member.UserId),
            cancellationToken);
}
