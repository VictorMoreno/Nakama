import type { AnalyticsSnapshot, QuestionActivity, UnhelpfulQuestion, UserActivity } from "./Analytics";

export interface IAnalyticsRepository {
  Find(): Promise<AnalyticsSnapshot>;
  FindUserActivity(days: number): Promise<UserActivity>;
  FindQuestionActivity(weeks: number): Promise<QuestionActivity>;
  FindUnhelpfulQuestions(): Promise<UnhelpfulQuestion[]>;
  FindUnhelpfulQuestionsOfChatbot(botKey: string): Promise<UnhelpfulQuestion[]>;
}
