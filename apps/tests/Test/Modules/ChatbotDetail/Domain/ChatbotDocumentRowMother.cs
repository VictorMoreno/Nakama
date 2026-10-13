using Nakama.ChatbotDetail.Domain;
using Test.Modules.Shared;

namespace Test.Modules.ChatbotDetail.Domain;

public static class ChatbotDocumentRowMother
{
    public static ChatbotDocumentRow Random() => With();

    public static ChatbotDocumentRow With(
        Guid? id = null,
        string? name = null,
        string? origin = null,
        string? status = null,
        string? statusLabel = null,
        DateTime? updated = null,
        string? fileName = null) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = name ?? WordMother.Random(),
            Origin = origin ?? WordMother.Random(),
            Status = status ?? "Ok",
            StatusLabel = statusLabel ?? WordMother.Random(),
            Updated = updated ?? DateTime.UtcNow,
            FileName = fileName ?? $"{Guid.NewGuid():N}.pdf"
        };
}
