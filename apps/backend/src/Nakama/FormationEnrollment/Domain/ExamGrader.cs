using Nakama.FormationEnrollment.Domain.Errors;

namespace Nakama.FormationEnrollment.Domain;

public sealed record ExamAnswerKey(int OptionCount, int CorrectIndex);

public static class ExamGrader
{
    public static int Score(IReadOnlyList<int> answers, IReadOnlyList<ExamAnswerKey> key)
    {
        if (key.Count == 0 || answers.Count != key.Count || answers.Where((answer, index) => answer < 0 || answer >= key[index].OptionCount).Any())
        {
            throw FormationEnrollmentErrors.InvalidExamAnswers;
        }

        int correct = answers.Where((answer, index) => answer == key[index].CorrectIndex).Count();

        return (int)Math.Round(correct * 100.0 / key.Count, MidpointRounding.AwayFromZero);
    }
}
