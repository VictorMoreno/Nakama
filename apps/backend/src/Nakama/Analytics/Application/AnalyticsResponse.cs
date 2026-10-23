using Nakama.Analytics.Domain;

namespace Nakama.Analytics.Application;

using Analytics = Nakama.Analytics.Domain.Analytics;

public sealed record AnalyticsResponse(
    string Id,
    int ActiveBotsCount,
    int AnsweredThisWeek,
    List<UsageByBotRow> UsageByBot,
    int ActiveUsersCount,
    int TotalUsersCount,
    int UsefulFeedbackCount,
    int TotalFeedbackCount);

public static class AnalyticsExtensions
{
    public static AnalyticsResponse ToResponse(this Analytics entity, int activeUsersCount, int totalUsersCount, int answeredThisWeek) => new(
        entity.Id,
        entity.ActiveBotsCount,
        answeredThisWeek,
        entity.UsageByBot,
        activeUsersCount,
        totalUsersCount,
        entity.UsefulFeedbackCount,
        entity.TotalFeedbackCount);
}
