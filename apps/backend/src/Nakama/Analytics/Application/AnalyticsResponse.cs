using Nakama.Analytics.Domain;

namespace Nakama.Analytics.Application;

using Analytics = Nakama.Analytics.Domain.Analytics;

public sealed record AnalyticsResponse(
    string Id,
    int ActiveBotsCount,
    int AnsweredThisWeek,
    List<UsageByBotRow> UsageByBot,
    int ActiveUsersCount,
    int TotalUsersCount);

public static class AnalyticsExtensions
{
    public static AnalyticsResponse ToResponse(this Analytics entity, int activeUsersCount, int totalUsersCount) => new(
        entity.Id,
        entity.ActiveBotsCount,
        entity.AnsweredThisWeek,
        entity.UsageByBot,
        activeUsersCount,
        totalUsersCount);
}
