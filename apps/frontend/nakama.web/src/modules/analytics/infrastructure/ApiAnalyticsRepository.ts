import type { AnalyticsResponse, GeneratedNakamaClient, UserActivityResponse } from "../../../shared/infrastructure/GeneratedNakamaClient";
import type { AnalyticsSnapshot, UserActivity } from "../domain/Analytics";
import type { IAnalyticsRepository } from "../domain/IAnalyticsRepository";

function toSnapshot(response: AnalyticsResponse): AnalyticsSnapshot {
  return {
    activeBotsCount: response.activeBotsCount ?? 0,
    answeredThisWeek: response.answeredThisWeek ?? 0,
    usageByBot: (response.usageByBot ?? []).map((row) => ({ botKey: row.botKey ?? "", value: row.value ?? 0 })),
    activeUsersCount: response.activeUsersCount ?? 0,
    totalUsersCount: response.totalUsersCount ?? 0,
    usefulFeedbackCount: response.usefulFeedbackCount ?? 0,
    totalFeedbackCount: response.totalFeedbackCount ?? 0
  };
}

function toUserActivity(response: UserActivityResponse): UserActivity {
  return {
    days: (response.days ?? []).map((row) => ({ date: row.date ?? "", logins: row.logins ?? 0, activeUsers: row.activeUsers ?? 0 })),
    activeToday: response.activeToday ?? 0,
    activeLast7Days: response.activeLast7Days ?? 0,
    activeLast30Days: response.activeLast30Days ?? 0
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

  async FindUserActivity(days: number): Promise<UserActivity> {
    const response = await this.client.userActivity(days);
    return toUserActivity(response);
  }

  async SubmitFeedback(useful: boolean): Promise<void> {
    await this.client.feedback({ useful });
  }
}
