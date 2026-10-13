using Nakama.ConversationHistory.Domain;
using Test.Modules.Shared;

namespace Test.Modules.ConversationHistory.Domain;

public static class HistoryExampleRowMother
{
    public static HistoryExampleRow With(
        string? question = null,
        string? answer = null,
        string? citation = null,
        string? moreExplanation = null,
        string? socraticVariant = null,
        List<string>? attachments = null) =>
        new()
        {
            Question = question ?? WordMother.Random(),
            Answer = answer ?? WordMother.Random(),
            Citation = citation ?? WordMother.Random(),
            MoreExplanation = moreExplanation ?? WordMother.Random(),
            SocraticVariant = socraticVariant ?? WordMother.Random(),
            Attachments = attachments ?? []
        };
}
