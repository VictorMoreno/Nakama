using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotDetail.Application.Handlers;

public sealed class ChatbotDetailUpdatedOnChatbotSettingsUpdated(IChatbotDetailRepository chatbotDetailRepository)
    : DomainEventHandler<ChatbotSettingsUpdated>
{
    protected override async Task Handle(ChatbotSettingsUpdated chatbotSettingsUpdated, CancellationToken cancellationToken)
    {
        Domain.ChatbotDetail? chatbotDetail = await chatbotDetailRepository.FindById(chatbotSettingsUpdated.Id.Value, cancellationToken);
        if (chatbotDetail is null)
        {
            return;
        }

        chatbotDetail.UpdateSettings(
            chatbotSettingsUpdated.Settings.Socratic,
            chatbotSettingsUpdated.Settings.ExplicaMas,
            chatbotSettingsUpdated.Settings.Provider.ToString(),
            chatbotSettingsUpdated.Settings.Model);
        await chatbotDetailRepository.Update(chatbotDetail, cancellationToken);
    }
}
