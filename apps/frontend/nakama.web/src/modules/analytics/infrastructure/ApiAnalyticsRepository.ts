import type { AnalyticsResponse, GeneratedNakamaClient } from "../../../shared/infrastructure/GeneratedNakamaClient";
import type { AnalyticsSnapshot } from "../domain/Analytics";
import type { IAnalyticsRepository } from "../domain/IAnalyticsRepository";

function toSnapshot(response: AnalyticsResponse): AnalyticsSnapshot {
  return {
    activeBotsCount: response.activeBotsCount ?? 0,
    answeredThisWeek: response.answeredThisWeek ?? 0,
    usageByBot: (response.usageByBot ?? []).map((row) => ({ botKey: row.botKey ?? "", value: row.value ?? 0 }))
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
