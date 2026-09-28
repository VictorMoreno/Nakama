import type { AnalyticsResponse, GeneratedNakamaClient } from "../../../shared/infrastructure/GeneratedNakamaClient";
import type { AnalyticsSnapshot } from "../domain/Analytics";
import type { IAnalyticsRepository } from "../domain/IAnalyticsRepository";

type AnalyticsResponseWithUserCounts = AnalyticsResponse & { activeUsersCount?: number; totalUsersCount?: number };

function toSnapshot(response: AnalyticsResponse): AnalyticsSnapshot {
  const withUserCounts = response as AnalyticsResponseWithUserCounts;
  return {
    activeBotsCount: response.activeBotsCount ?? 0,
    answeredThisWeek: response.answeredThisWeek ?? 0,
    usageByBot: (response.usageByBot ?? []).map((row) => ({ botKey: row.botKey ?? "", value: row.value ?? 0 })),
    activeUsersCount: withUserCounts.activeUsersCount ?? 0,
    totalUsersCount: withUserCounts.totalUsersCount ?? 0
  };
}

export class ApiAnalyticsRepository implements IAnalyticsRepository {
  private readonly client: GeneratedNakamaClient;

  constructor(client: GeneratedNakamaClient) {
    this.client = client;
  }

  async Find(): Promise<AnalyticsSnapshot> {
    const response = await this.client.analytics();
    return toSnapshot(response);
  }
}
