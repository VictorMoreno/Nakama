using Nakama.Analytics.Domain;
using Test.Modules.Shared;
using AnalyticsEntity = Nakama.Analytics.Domain.Analytics;

namespace Test.Modules.Analytics.Domain;

public static class AnalyticsMother
{
    public static AnalyticsEntity Create(
        string id,
        int activeBotsCount,
        int answeredThisWeek,
        List<UsageByBotRow> usageByBot,
        int usefulFeedbackCount,
        int totalFeedbackCount) =>
        new(id, activeBotsCount, answeredThisWeek, usageByBot, usefulFeedbackCount, totalFeedbackCount);

    public static AnalyticsEntity Random() => With();

    public static AnalyticsEntity WithUsageRow(int? activeBotsCount = null, int? answeredThisWeek = null) =>
        With(
            activeBotsCount: activeBotsCount,
            answeredThisWeek: answeredThisWeek,
            usageByBot: [UsageByBotRowMother.Random()]);

    public static AnalyticsEntity With(
        string? id = null,
        int? activeBotsCount = null,
        int? answeredThisWeek = null,
        List<UsageByBotRow>? usageByBot = null,
        int? usefulFeedbackCount = null,
        int? totalFeedbackCount = null) =>
        Create(
            id ?? AnalyticsEntity.SingletonKey,
            activeBotsCount ?? MotherCreator.Random.Random.Number(1, 20),
            answeredThisWeek ?? MotherCreator.Random.Random.Number(1, 100),
            usageByBot ?? [],
            usefulFeedbackCount ?? MotherCreator.Random.Random.Number(1, 10),
            totalFeedbackCount ?? MotherCreator.Random.Random.Number(10, 30));
}
