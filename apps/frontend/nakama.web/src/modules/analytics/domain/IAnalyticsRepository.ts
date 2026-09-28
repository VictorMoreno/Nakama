import type { AnalyticsSnapshot } from "./Analytics";

export interface IAnalyticsRepository {
  Find(): Promise<AnalyticsSnapshot>;
  SubmitFeedback(useful: boolean): Promise<void>;
}
