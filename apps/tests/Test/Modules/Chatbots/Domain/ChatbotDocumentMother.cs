using Nakama.Chatbots.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Chatbots.Domain;

public static class ChatbotDocumentMother
{
    public static ChatbotDocument Random() => With();

    public static ChatbotDocument With(
        Guid? id = null,
        string? name = null,
        string? origin = null,
        DocStatus? status = null,
        string? statusLabel = null,
        DateTime? updated = null,
        string? fileName = null) =>
        new(
            id ?? Guid.NewGuid(),
            name ?? WordMother.Random(),
            origin ?? WordMother.Random(),
            status ?? DocStatus.Ok,
            statusLabel ?? WordMother.Random(),
            updated ?? DateTime.UtcNow,
            fileName ?? $"{Guid.NewGuid():N}.pdf");
}
