using MediatR;
using Nakama.ChatPage.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatPage.Application.Handlers;

using ChatPage = Nakama.ChatPage.Domain.ChatPage;

/// <summary>Updates <see cref="ChatPage"/> from <see cref="ChatbotSettingsUpdated"/>.</summary>
public sealed class ChatPageUpdatedOnChatbotSettingsUpdated(IChatPageRepository chatPageRepository)
    : INotificationHandler<DomainEventNotification<ChatbotSettingsUpdated>>
{
    public async Task Handle(DomainEventNotification<ChatbotSettingsUpdated> n, CancellationToken ct)
    {
        var entry = await chatPageRepository.FindByBotKey(n.DomainEvent.Key.Value, ct);
        if (entry is null)
        {
            return;
        }

        entry.UpdateSettings(
            n.DomainEvent.Settings.Socratic,
            n.DomainEvent.Settings.ExplicaMas,
            n.DomainEvent.Settings.Provider.ToString());
        await chatPageRepository.Save(entry, ct);
    }
}
