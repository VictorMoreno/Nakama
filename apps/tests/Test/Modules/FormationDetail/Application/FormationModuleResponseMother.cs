using Nakama.FormationDetail.Application;
using Nakama.Formations.Domain;
using Test.Modules.Shared;

namespace Test.Modules.FormationDetail.Application;

public static class FormationModuleResponseMother
{
    public static FormationModuleResponse Random() => With();

    public static FormationModuleResponse Exam(int passingScore, params int[] correctIndexes) =>
        With(
            type: ModuleType.Exam.ToString(),
            passingScore: passingScore,
            quiz: correctIndexes
                .Select(correctIndex => new QuizQuestionResponse(
                    WordMother.Random(),
                    [WordMother.Random(), WordMother.Random(), WordMother.Random()],
                    correctIndex))
                .ToList());

    public static FormationModuleResponse With(
        Guid? id = null,
        string? type = null,
        bool? gated = null,
        List<QuizQuestionResponse>? quiz = null,
        int? passingScore = null) =>
        new(
            id ?? Guid.NewGuid(),
            WordMother.Random(),
            type ?? ModuleType.Document.ToString(),
            WordMother.Random(),
            gated ?? false,
            false,
            null,
            null,
            quiz,
            passingScore);
}
