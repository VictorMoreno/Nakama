import type { AnalyticsSnapshot } from "./Analytics";

export interface IAnalyticsRepository {
  Find(): Promise<AnalyticsSnapshot>;
}
