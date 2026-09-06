using MediatR;
using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotDetail.Application.Handlers;

using ChatbotDetail = Nakama.ChatbotDetail.Domain.ChatbotDetail;

/// <summary>Updates <see cref="ChatbotDetail"/> from <see cref="ChatbotSettingsUpdated"/>.</summary>
public sealed class ChatbotDetailUpdatedOnChatbotSettingsUpdated(IChatbotDetailRepository chatbotDetailRepository)
    : INotificationHandler<DomainEventNotification<ChatbotSettingsUpdated>>
{
    public async Task Handle(DomainEventNotification<ChatbotSettingsUpdated> n, CancellationToken ct)
    {
        var entry = await chatbotDetailRepository.FindByKey(n.DomainEvent.Key.Value, ct);
        if (entry is null)
        {
            return;
        }

        entry.UpdateSettings(
            n.DomainEvent.Settings.Socratic,
            n.DomainEvent.Settings.ExplicaMas,
            n.DomainEvent.Settings.Provider.ToString());
        await chatbotDetailRepository.Save(entry, ct);
    }
}
