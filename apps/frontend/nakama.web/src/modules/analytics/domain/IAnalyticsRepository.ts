import type { AnalyticsSnapshot, QuestionActivity, UserActivity } from "./Analytics";

export interface IAnalyticsRepository {
  Find(): Promise<AnalyticsSnapshot>;
  FindUserActivity(days: number): Promise<UserActivity>;
  FindQuestionActivity(weeks: number): Promise<QuestionActivity>;
  SubmitFeedback(useful: boolean): Promise<void>;
}
