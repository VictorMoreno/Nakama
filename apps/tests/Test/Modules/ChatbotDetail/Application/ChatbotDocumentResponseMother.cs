using Nakama.ChatbotDetail.Application;
using Test.Modules.Shared;

namespace Test.Modules.ChatbotDetail.Application;

public static class ChatbotDocumentResponseMother
{
    public static ChatbotDocumentResponse Random() => With();

    public static ChatbotDocumentResponse With(string? name = null, string? fileName = null) =>
        new(
            Guid.NewGuid(),
            name ?? WordMother.Random(),
            WordMother.Random(),
            "Ok",
            WordMother.Random(),
            DateTime.UtcNow,
            fileName ?? $"{Guid.NewGuid():N}.pdf");
}
