using Nakama.Shared.Infrastructure.Reporting;

namespace Nakama.Shared.Infrastructure.AnswerFeedback;

public sealed class AnswerFeedbackOptions
{
    public const string SectionName = ReportingOptions.SectionName + ":AnswerFeedback";

    public required int UnhelpfulQuestionsLimit { get; init; }
}
