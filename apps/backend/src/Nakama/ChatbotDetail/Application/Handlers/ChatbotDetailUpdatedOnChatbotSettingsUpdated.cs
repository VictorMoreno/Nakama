using Nakama.Chatbots.Domain.Events;
using Nakama.ChatbotDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.ChatbotDetail.Application.Handlers;

using ChatbotDetail = Nakama.ChatbotDetail.Domain.ChatbotDetail;

public sealed class ChatbotDetailUpdatedOnChatbotSettingsUpdated(IChatbotDetailRepository chatbotDetailRepository)
    : DomainEventHandler<ChatbotSettingsUpdated>
{
    protected override async Task Handle(ChatbotSettingsUpdated chatbotSettingsUpdated, CancellationToken ct)
    {
        var chatbotDetail = await chatbotDetailRepository.FindById(chatbotSettingsUpdated.Id.Value, ct);
        if (chatbotDetail is null)
        {
            return;
        }

        chatbotDetail.UpdateSettings(
            chatbotSettingsUpdated.Settings.Socratic,
            chatbotSettingsUpdated.Settings.ExplicaMas,
            chatbotSettingsUpdated.Settings.Provider.ToString());
        await chatbotDetailRepository.Update(chatbotDetail, ct);
    }
}
