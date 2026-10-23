import { useEffect, useState } from "react";
import type { QuestionActivity } from "../../../../modules/analytics/domain/Analytics";
import type { IAnalyticsRepository } from "../../../../modules/analytics/domain/IAnalyticsRepository";
import { questionActivityConfig } from "../../../../shared/infrastructure/config/QuestionActivityConfig";
import { Services } from "../../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../../shared/infrastructure/injection/useInjection";

const EMPTY_ACTIVITY: QuestionActivity = { weeks: [], questionsThisWeek: 0 };

export const WEEK_RANGE_OPTIONS: { value: string; label: string }[] = questionActivityConfig.weekOptions.map((weeks) => ({
  value: String(weeks),
  label: `Últimas ${weeks} semanas`
}));

export function useQuestionActivity() {
  const repo = useInjection<IAnalyticsRepository>(Services.AnalyticsRepository.id);
  const [weeks, setWeeks] = useState<string>(String(questionActivityConfig.defaultWeeks));
  const [activity, setActivity] = useState<QuestionActivity>(EMPTY_ACTIVITY);

  useEffect(() => {
    let cancelled = false;
    repo.FindQuestionActivity(Number(weeks)).then((data) => {
      if (!cancelled) setActivity(data);
    });
    return () => {
      cancelled = true;
    };
  }, [repo, weeks]);

  return { activity, weeks, setWeeks };
}
