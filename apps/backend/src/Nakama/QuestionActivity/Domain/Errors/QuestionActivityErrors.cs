using Nakama.Shared.Domain;

namespace Nakama.QuestionActivity.Domain.Errors;

public sealed class QuestionActivityInvalidWeeksError(int maxWeeks)
    : DomainError("nakama.question_activity.invalid_weeks", $"The number of weeks must be between 1 and {maxWeeks}.");

public static class QuestionActivityErrors
{
    public static DomainError InvalidWeeks(int maxWeeks) => new QuestionActivityInvalidWeeksError(maxWeeks);
}
