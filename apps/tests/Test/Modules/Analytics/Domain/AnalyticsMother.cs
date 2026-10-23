using Test.Modules.Shared;
using AnalyticsEntity = Nakama.Analytics.Domain.Analytics;

namespace Test.Modules.Analytics.Domain;

public static class AnalyticsMother
{
    public static AnalyticsEntity Create(
        string id,
        int activeBotsCount,
        int usefulFeedbackCount,
        int totalFeedbackCount) =>
        new(id, activeBotsCount, usefulFeedbackCount, totalFeedbackCount);

    public static AnalyticsEntity Random() => With();

    public static AnalyticsEntity With(
        string? id = null,
        int? activeBotsCount = null,
        int? usefulFeedbackCount = null,
        int? totalFeedbackCount = null) =>
        Create(
            id ?? AnalyticsEntity.SingletonKey,
            activeBotsCount ?? MotherCreator.Random.Random.Number(1, 20),
            usefulFeedbackCount ?? MotherCreator.Random.Random.Number(1, 10),
            totalFeedbackCount ?? MotherCreator.Random.Random.Number(10, 30));
}
