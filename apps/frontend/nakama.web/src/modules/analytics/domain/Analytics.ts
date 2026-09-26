export interface UsageByBot {
  botKey: string;
  value: number;
}

export interface AnalyticsSnapshot {
  activeBotsCount: number;
  answeredThisWeek: number;
  usageByBot: UsageByBot[];
}
