using Nakama.Shared.Infrastructure.Chat;

namespace Test.Modules.Shared;

public static class ChatOptionsMother
{
    public static ChatOptions Random() => With();

    public static ChatOptions With(int? maxAttachmentsPerMessage = null) =>
        new()
        {
            MaxAttachmentsPerMessage = maxAttachmentsPerMessage ?? MotherCreator.Random.Random.Number(2, 5),
            MaxAttachmentSizeMb = MotherCreator.Random.Random.Number(1, 20),
            AllowedAttachmentExtensions = [".pdf", ".md", ".txt"]
        };
}
