using Nakama.Formations.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Formations.Domain;

public static class QuizQuestionMother
{
    public static QuizQuestion Random() => With();

    public static QuizQuestion With(string? question = null, IReadOnlyList<string>? options = null, int? correctIndex = null) =>
        new(
            question ?? WordMother.Random(),
            options ?? [WordMother.Random(), WordMother.Random()],
            correctIndex ?? 0);
}
