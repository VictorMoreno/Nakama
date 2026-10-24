using Nakama.AnswerFeedback.Application;
using Nakama.QuestionActivity.Application;

namespace Nakama.Analytics.Application;

public sealed record UsageByBotRow(Guid BotKey, int Value);

public sealed record AnalyticsResponse(
    int AnsweredThisWeek,
    List<UsageByBotRow> UsageByBot,
    int ActiveUsersCount,
    int TotalUsersCount,
    int UsefulFeedbackCount,
    int TotalFeedbackCount)
{
    public static AnalyticsResponse From(
        int activeUsersCount,
        int totalUsersCount,
        int answeredThisWeek,
        QuestionsByChatbotResponse byChatbot,
        AnswerFeedbackSummaryResponse feedback) => new(
        answeredThisWeek,
        byChatbot.Chatbots.Select(row => new UsageByBotRow(row.BotKey, row.Questions)).ToList(),
        activeUsersCount,
        totalUsersCount,
        feedback.Useful,
        feedback.Total);
}
