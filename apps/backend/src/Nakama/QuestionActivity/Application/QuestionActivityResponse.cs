namespace Nakama.QuestionActivity.Application;

public sealed record WeeklyQuestionRow(string WeekStart, int Questions);

public sealed record QuestionActivityResponse(IReadOnlyList<WeeklyQuestionRow> Weeks, int QuestionsThisWeek);
