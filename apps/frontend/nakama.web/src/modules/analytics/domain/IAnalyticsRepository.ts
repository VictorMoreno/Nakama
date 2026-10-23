import type { AnalyticsSnapshot, UserActivity } from "./Analytics";

export interface IAnalyticsRepository {
  Find(): Promise<AnalyticsSnapshot>;
  FindUserActivity(days: number): Promise<UserActivity>;
  SubmitFeedback(useful: boolean): Promise<void>;
}
