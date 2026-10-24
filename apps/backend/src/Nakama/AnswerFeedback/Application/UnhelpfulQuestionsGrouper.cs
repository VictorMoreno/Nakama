using Nakama.AnswerFeedback.Domain;

namespace Nakama.AnswerFeedback.Application;

internal static class UnhelpfulQuestionsGrouper
{
    public static UnhelpfulQuestionsResponse Group(IEnumerable<AnswerFeedbackEntry> entries, int limit)
    {
        List<UnhelpfulQuestionRow> rows = entries
            .GroupBy(entry => (entry.BotKey, Key: entry.Question.Trim().ToLowerInvariant()))
            .Select(group => new UnhelpfulQuestionRow(group.Key.BotKey, group.First().Question.Trim(), group.Count()))
            .OrderByDescending(row => row.Count)
            .ThenBy(row => row.Question, StringComparer.OrdinalIgnoreCase)
            .ThenBy(row => row.BotKey)
            .Take(limit)
            .ToList();

        return new UnhelpfulQuestionsResponse(rows);
    }
}
