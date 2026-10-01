/** Injection tokens — pass `Services.X.id` to `useInjection<IX>(...)` to resolve X. */
export const Services = {
  AnalyticsRepository: { id: "AnalyticsRepository" },
  ChatbotsRepository: { id: "ChatbotsRepository" },
  ConversationsRepository: { id: "ConversationsRepository" },
  FormationsRepository: { id: "FormationsRepository" },
  FormationEnrollmentRepository: { id: "FormationEnrollmentRepository" },
  UsersRepository: { id: "UsersRepository" }
} as const;
