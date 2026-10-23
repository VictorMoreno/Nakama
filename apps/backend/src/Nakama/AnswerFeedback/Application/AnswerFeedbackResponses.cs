namespace Nakama.AnswerFeedback.Application;

public sealed record AnswerFeedbackSummaryResponse(int Useful, int Total);

public sealed record UnhelpfulQuestionRow(Guid BotKey, string Question, int Count);

public sealed record UnhelpfulQuestionsResponse(IReadOnlyList<UnhelpfulQuestionRow> Questions);
