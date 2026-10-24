import { useEffect, useState } from "react";
import type { AnalyticsSnapshot, UnhelpfulQuestion } from "../../../../modules/analytics/domain/Analytics";
import type { IAnalyticsRepository } from "../../../../modules/analytics/domain/IAnalyticsRepository";
import { Services } from "../../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../../shared/infrastructure/injection/useInjection";
import { useChatbots } from "../../../shared/contexts/ChatbotsContext";

const EMPTY_SNAPSHOT: AnalyticsSnapshot = {
  answeredThisWeek: 0,
  usageByBot: [],
  activeUsersCount: 0,
  totalUsersCount: 0,
  usefulFeedbackCount: 0,
  totalFeedbackCount: 0
};

export function useAnalytics() {
  const { chatbots } = useChatbots();
  const repo = useInjection<IAnalyticsRepository>(Services.AnalyticsRepository.id);
  const [snapshot, setSnapshot] = useState<AnalyticsSnapshot>(EMPTY_SNAPSHOT);
  const [unhelpful, setUnhelpful] = useState<UnhelpfulQuestion[]>([]);

  useEffect(() => {
    let cancelled = false;
    repo.Find().then((data) => {
      if (!cancelled) setSnapshot(data);
    });
    repo.FindUnhelpfulQuestions().then((data) => {
      if (!cancelled) setUnhelpful(data);
    });
    return () => {
      cancelled = true;
    };
  }, [repo]);

  const unhelpfulQuestions = unhelpful.map((row) => ({ ...row, dept: chatbots[row.botKey]?.label ?? row.botKey }));

  return { unhelpfulQuestions, snapshot };
}
