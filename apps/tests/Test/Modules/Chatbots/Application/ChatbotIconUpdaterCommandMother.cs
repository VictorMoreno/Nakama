using Nakama.Chatbots.Application.IconUpdater;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.Chatbots.Application;

public static class ChatbotIconUpdaterCommandMother
{
    public static ChatbotIconUpdater.Command WithUpload(Guid? id = null, string contentType = "image/png")
    {
        byte[] content = [1, 2, 3];

        return new ChatbotIconUpdater.Command(
            id ?? ChatbotMother.RandomId().Value,
            new ChatbotIconUpdater.Upload(contentType, new MemoryStream(content), content.Length));
    }

    public static ChatbotIconUpdater.Command WithoutUpload(Guid? id = null) =>
        new(id ?? ChatbotMother.RandomId().Value, null);
}
