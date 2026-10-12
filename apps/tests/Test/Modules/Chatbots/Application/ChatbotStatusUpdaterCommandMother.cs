using Nakama.Chatbots.Application.StatusUpdater;
using Nakama.Chatbots.Domain;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.Chatbots.Application;

public static class ChatbotStatusUpdaterCommandMother
{
    public static ChatbotStatusUpdater.Command Random() => With();

    public static ChatbotStatusUpdater.Command With(Guid? id = null, ChatbotStatus? status = null) =>
        new(id ?? ChatbotMother.RandomId().Value, (int)(status ?? ChatbotStatus.Active));
}
