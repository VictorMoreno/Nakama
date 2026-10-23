using Test.Modules.Shared;
using AnalyticsEntity = Nakama.Analytics.Domain.Analytics;

namespace Test.Modules.Analytics.Domain;

public static class AnalyticsMother
{
    public static AnalyticsEntity Create(
        string id,
        int activeBotsCount) =>
        new(id, activeBotsCount);

    public static AnalyticsEntity Random() => With();

    public static AnalyticsEntity With(
        string? id = null,
        int? activeBotsCount = null) =>
        Create(
            id ?? AnalyticsEntity.SingletonKey,
            activeBotsCount ?? MotherCreator.Random.Random.Number(1, 20));
}
