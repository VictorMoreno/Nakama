import { useEffect, useState } from "react";
import type { UnhelpfulQuestion } from "../../../../../modules/analytics/domain/Analytics";
import type { IAnalyticsRepository } from "../../../../../modules/analytics/domain/IAnalyticsRepository";
import { Services } from "../../../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../../../shared/infrastructure/injection/useInjection";

export function useAnalyticsTab(botKey: string) {
  const repo = useInjection<IAnalyticsRepository>(Services.AnalyticsRepository.id);
  const [unhelpfulQuestions, setUnhelpfulQuestions] = useState<UnhelpfulQuestion[] | null>(null);

  useEffect(() => {
    let cancelled = false;
    repo.FindUnhelpfulQuestionsOfChatbot(botKey).then((data) => {
      if (!cancelled) setUnhelpfulQuestions(data);
    });
    return () => {
      cancelled = true;
    };
  }, [repo, botKey]);

  return { unhelpfulQuestions };
}
