using MediatR;
using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotDetail.Application.Handlers;

using ChatbotDetail = Nakama.ChatbotDetail.Domain.ChatbotDetail;

/// <summary>Updates <see cref="ChatbotDetail"/> from <see cref="ChatbotTeamMemberRemoved"/>.</summary>
public sealed class ChatbotDetailUpdatedOnChatbotTeamMemberRemoved(IChatbotDetailRepository chatbotDetailRepository)
    : INotificationHandler<DomainEventNotification<ChatbotTeamMemberRemoved>>
{
    public async Task Handle(DomainEventNotification<ChatbotTeamMemberRemoved> n, CancellationToken ct)
    {
        var entry = await chatbotDetailRepository.FindByKey(n.DomainEvent.Key.Value, ct);
        if (entry is null)
        {
            return;
        }

        entry.RemoveTeamMember(n.DomainEvent.Index);
        await chatbotDetailRepository.Save(entry, ct);
    }
}
