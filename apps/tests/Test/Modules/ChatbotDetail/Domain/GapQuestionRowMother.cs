using Nakama.ChatbotDetail.Domain;
using Test.Modules.Shared;

namespace Test.Modules.ChatbotDetail.Domain;

public static class GapQuestionRowMother
{
    public static GapQuestionRow Random() => With();

    public static GapQuestionRow With(string? question = null, int? count = null) =>
        new() { Question = question ?? WordMother.Random(), Count = count ?? MotherCreator.Random.Random.Number(1, 20) };
}
