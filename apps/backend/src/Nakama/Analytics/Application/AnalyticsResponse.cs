using Nakama.Analytics.Domain;

namespace Nakama.Analytics.Application;

using Analytics = Nakama.Analytics.Domain.Analytics;

public sealed record AnalyticsResponse(
    string Key,
    int ActiveBotsCount,
    int AnsweredThisWeek,
    List<UsageByBotRow> UsageByBot)
{
    public static explicit operator AnalyticsResponse(Analytics entity) => new(
        entity.Key,
        entity.ActiveBotsCount,
        entity.AnsweredThisWeek,
        entity.UsageByBot);
}
