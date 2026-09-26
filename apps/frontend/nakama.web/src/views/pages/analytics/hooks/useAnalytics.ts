import { useEffect, useState } from "react";
import type { AnalyticsSnapshot } from "../../../../modules/analytics/domain/Analytics";
import type { IAnalyticsRepository } from "../../../../modules/analytics/domain/IAnalyticsRepository";
import { Services } from "../../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../../shared/infrastructure/injection/useInjection";
import { useChatbots } from "../../../shared/contexts/ChatbotsContext";

const EMPTY_SNAPSHOT: AnalyticsSnapshot = { activeBotsCount: 0, answeredThisWeek: 0, usageByBot: [] };

export function useAnalytics() {
  const { chatbots } = useChatbots();
  const repo = useInjection<IAnalyticsRepository>(Services.AnalyticsRepository.id);
  const [snapshot, setSnapshot] = useState<AnalyticsSnapshot>(EMPTY_SNAPSHOT);

  useEffect(() => {
    let cancelled = false;
    repo.Find().then((data) => {
      if (!cancelled) setSnapshot(data);
    });
    return () => {
      cancelled = true;
    };
  }, [repo]);

  const globalGaps = Object.values(chatbots)
    .filter((bot) => !bot.archived)
    .flatMap((bot) => bot.gaps.map((g) => ({ ...g, dept: bot.label })))
    .sort((a, b) => b.count - a.count);

  return { chatbots, globalGaps, snapshot };
}
