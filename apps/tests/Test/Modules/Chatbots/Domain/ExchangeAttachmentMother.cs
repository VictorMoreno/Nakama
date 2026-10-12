using Nakama.Chatbots.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Chatbots.Domain;

public static class ExchangeAttachmentMother
{
    public static ExchangeAttachment Random() => With();

    public static ExchangeAttachment With(string? name = null, string? fileName = null) =>
        new(
            name ?? $"{WordMother.Random()}.pdf",
            fileName ?? $"{Guid.NewGuid():N}.pdf");
}
