using Nakama.ChatPage.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatPage.Application.Handlers;

public sealed class ChatPageUpdatedOnChatbotSettingsUpdated(IChatPageRepository chatPageRepository)
    : DomainEventHandler<ChatbotSettingsUpdated>
{
    protected override async Task Handle(ChatbotSettingsUpdated chatbotSettingsUpdated, CancellationToken cancellationToken)
    {
        Domain.ChatPage? chatPage = await chatPageRepository.FindById(chatbotSettingsUpdated.Id.Value, cancellationToken);
        if (chatPage is null)
        {
            return;
        }

        chatPage.UpdateSettings(
            chatbotSettingsUpdated.Settings.Socratic,
            chatbotSettingsUpdated.Settings.ExplicaMas,
            chatbotSettingsUpdated.Settings.Provider.ToString());
        await chatPageRepository.Update(chatPage, cancellationToken);
    }
}
