using Nakama.ChatbotDetail.Application;
using Test.Modules.Shared;

namespace Test.Modules.ChatbotDetail.Application;

public static class ChatbotDetailResponseMother
{
    public static ChatbotDetailResponse Random() => With();

    public static ChatbotDetailResponse With(
        Guid? id = null,
        string? label = null,
        string? description = null,
        string? provider = null,
        string? model = null,
        bool? socratic = null,
        bool? explicaMas = null,
        List<ChatbotDocumentResponse>? docs = null) =>
        new(
            id ?? Guid.NewGuid(),
            label ?? WordMother.Random(),
            description ?? WordMother.Random(),
            "Active",
            WordMother.Random(),
            false,
            null,
            socratic ?? false,
            explicaMas ?? false,
            provider ?? "Anthropic",
            model ?? WordMother.Random(),
            docs ?? [],
            []);
}
