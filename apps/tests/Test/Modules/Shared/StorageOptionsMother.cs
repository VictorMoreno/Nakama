using Nakama.Shared.Infrastructure.Storage;

namespace Test.Modules.Shared;

public static class StorageOptionsMother
{
    public static StorageOptions Random() => With();

    public static StorageOptions With(string? chatbotIconsRoute = null, string? formationIconsRoute = null) =>
        new()
        {
            ChatbotIconsPath = WordMother.Random(),
            ChatbotIconsRoute = chatbotIconsRoute ?? $"/{WordMother.Random()}",
            FormationIconsPath = WordMother.Random(),
            FormationIconsRoute = formationIconsRoute ?? $"/{WordMother.Random()}",
            ChatbotDocumentsPath = WordMother.Random(),
            ConversationAttachmentsPath = WordMother.Random()
        };
}
