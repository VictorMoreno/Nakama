import type { AnalyticsResponse, GeneratedNakamaClient, QuestionActivityResponse, UnhelpfulQuestionsResponse, UserActivityResponse } from "../../../shared/infrastructure/GeneratedNakamaClient";
import type { AnalyticsSnapshot, QuestionActivity, UnhelpfulQuestion, UserActivity } from "../domain/Analytics";
import type { IAnalyticsRepository } from "../domain/IAnalyticsRepository";

function toSnapshot(response: AnalyticsResponse): AnalyticsSnapshot {
  return {
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

function toQuestionActivity(response: QuestionActivityResponse): QuestionActivity {
  return {
    weeks: (response.weeks ?? []).map((row) => ({ weekStart: row.weekStart ?? "", questions: row.questions ?? 0 })),
    questionsThisWeek: response.questionsThisWeek ?? 0
  };
}

function toUnhelpfulQuestions(response: UnhelpfulQuestionsResponse): UnhelpfulQuestion[] {
  return (response.questions ?? []).map((row) => ({ botKey: row.botKey ?? "", question: row.question ?? "", count: row.count ?? 0 }));
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

  async FindQuestionActivity(weeks: number): Promise<QuestionActivity> {
    const response = await this.client.questionActivity(weeks);
    return toQuestionActivity(response);
  }

  async FindUnhelpfulQuestions(): Promise<UnhelpfulQuestion[]> {
    const response = await this.client.unhelpfulQuestions();
    return toUnhelpfulQuestions(response);
  }

  async FindUnhelpfulQuestionsOfChatbot(botKey: string): Promise<UnhelpfulQuestion[]> {
    const response = await this.client.unhelpfulQuestions2(botKey);
    return toUnhelpfulQuestions(response);
  }
}
