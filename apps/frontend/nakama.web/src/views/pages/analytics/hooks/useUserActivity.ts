import { useEffect, useState } from "react";
import type { UserActivity } from "../../../../modules/analytics/domain/Analytics";
import type { IAnalyticsRepository } from "../../../../modules/analytics/domain/IAnalyticsRepository";
import { userActivityConfig } from "../../../../shared/infrastructure/config/UserActivityConfig";
import { Services } from "../../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../../shared/infrastructure/injection/useInjection";

const EMPTY_ACTIVITY: UserActivity = { days: [], activeToday: 0, activeLast7Days: 0, activeLast30Days: 0 };

export const DAY_RANGE_OPTIONS: { value: string; label: string }[] = userActivityConfig.dayOptions.map((days) => ({
  value: String(days),
  label: `Últimos ${days} días`
}));

export function useUserActivity() {
  const repo = useInjection<IAnalyticsRepository>(Services.AnalyticsRepository.id);
  const [days, setDays] = useState<string>(String(userActivityConfig.defaultDays));
  const [activity, setActivity] = useState<UserActivity>(EMPTY_ACTIVITY);

  useEffect(() => {
    let cancelled = false;
    repo.FindUserActivity(Number(days)).then((data) => {
      if (!cancelled) setActivity(data);
    });
    return () => {
      cancelled = true;
    };
  }, [repo, days]);

  return { activity, days, setDays };
}
