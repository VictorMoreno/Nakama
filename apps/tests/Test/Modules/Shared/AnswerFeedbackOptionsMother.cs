using Nakama.Shared.Infrastructure.AnswerFeedback;

namespace Test.Modules.Shared;

public static class AnswerFeedbackOptionsMother
{
    public static AnswerFeedbackOptions Random() => With();

    public static AnswerFeedbackOptions With(int? unhelpfulQuestionsLimit = null) =>
        new()
        {
            UnhelpfulQuestionsLimit = unhelpfulQuestionsLimit ?? 10
        };
}
