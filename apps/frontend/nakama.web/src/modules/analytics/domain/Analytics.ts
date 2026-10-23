interface UsageByBot {
  botKey: string;
  value: number;
}

export interface AnalyticsSnapshot {
  activeBotsCount: number;
  answeredThisWeek: number;
  usageByBot: UsageByBot[];
  activeUsersCount: number;
  totalUsersCount: number;
  usefulFeedbackCount: number;
  totalFeedbackCount: number;
}

export interface DailyUserActivity {
  date: string;
  logins: number;
  activeUsers: number;
}

export interface UserActivity {
  days: DailyUserActivity[];
  activeToday: number;
  activeLast7Days: number;
  activeLast30Days: number;
}

export interface WeeklyQuestions {
  weekStart: string;
  questions: number;
}

export interface QuestionActivity {
  weeks: WeeklyQuestions[];
  questionsThisWeek: number;
}
