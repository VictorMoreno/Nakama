using Nakama.ConversationHistory.Domain;
using Test.Modules.Shared;

namespace Test.Modules.ConversationHistory.Domain;

public static class HistoryExampleRowMother
{
    public static HistoryExampleRow Random() => With();

    public static HistoryExampleRow With(
        string? question = null,
        string? answer = null,
        List<string>? attachments = null) =>
        new()
        {
            Question = question ?? WordMother.Random(),
            Answer = answer ?? WordMother.Random(),
            Attachments = attachments ?? []
        };
}
