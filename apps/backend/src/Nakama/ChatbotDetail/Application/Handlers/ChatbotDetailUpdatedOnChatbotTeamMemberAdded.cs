using MediatR;
using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotDetail.Application.Handlers;

using ChatbotDetail = Nakama.ChatbotDetail.Domain.ChatbotDetail;

/// <summary>Updates <see cref="ChatbotDetail"/> from <see cref="ChatbotTeamMemberAdded"/>.</summary>
public sealed class ChatbotDetailUpdatedOnChatbotTeamMemberAdded(IChatbotDetailRepository chatbotDetailRepository)
    : INotificationHandler<DomainEventNotification<ChatbotTeamMemberAdded>>
{
    public async Task Handle(DomainEventNotification<ChatbotTeamMemberAdded> n, CancellationToken ct)
    {
        var entry = await chatbotDetailRepository.FindByKey(n.DomainEvent.Key.Value, ct);
        if (entry is null)
        {
            return;
        }

        entry.AddTeamMember(n.DomainEvent.Member.Name, n.DomainEvent.Member.Role.ToString());
        await chatbotDetailRepository.Save(entry, ct);
    }
}
