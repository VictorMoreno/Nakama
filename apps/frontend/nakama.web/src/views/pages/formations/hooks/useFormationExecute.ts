import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import type { LearnerProgress } from "../../../../modules/formations/domain/Formation";
import type { IFormationEnrollmentRepository } from "../../../../modules/formations/domain/IFormationEnrollmentRepository";
import { Services } from "../../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../../shared/infrastructure/injection/useInjection";
import { useFormations } from "../../../shared/contexts/FormationsContext";
import { useUser } from "../../../shared/contexts/UserContext";

const EMPTY_PROGRESS: LearnerProgress = { completedModuleIds: [], examScores: {}, examAttempts: [] };

export function useFormationExecute() {
  const { key } = useParams<{ key: string }>();
  const { formations } = useFormations();
  const { currentUser } = useUser();
  const enrollmentRepo = useInjection<IFormationEnrollmentRepository>(Services.FormationEnrollmentRepository.id);
  const navigate = useNavigate();

  const c = key ? formations[key] : undefined;
  const [loadedProgress, setLoadedProgress] = useState<LearnerProgress>(EMPTY_PROGRESS);
  const myProgress: LearnerProgress = c && currentUser ? loadedProgress : EMPTY_PROGRESS;

  useEffect(() => {
    if (!c || !currentUser) return;

    let cancelled = false;
    enrollmentRepo.FindMyProgress(c.id).then((progress) => {
      if (!cancelled) setLoadedProgress(progress ?? EMPTY_PROGRESS);
    });
    return () => {
      cancelled = true;
    };
  }, [c, currentUser, enrollmentRepo]);

  const pct = c?.progressPct ?? 0;

  function isLocked(idx: number): boolean {
    if (!c || idx === 0) return false;
    const prev = c.modules[idx - 1];
    return prev.gated && !myProgress.completedModuleIds.includes(prev.id);
  }

  return { c, navigate, pct, isLocked, myProgress };
}
