using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;

namespace Test.Modules.Chatbots.Domain;

public static class ChatbotSettingsUpdatedMother
{
    public static ChatbotSettingsUpdated Random() => With();

    public static ChatbotSettingsUpdated With(ChatbotId? id = null, ChatbotSettings? settings = null) =>
        new(id ?? ChatbotMother.RandomId(), settings ?? ChatbotSettingsMother.Random());
}
