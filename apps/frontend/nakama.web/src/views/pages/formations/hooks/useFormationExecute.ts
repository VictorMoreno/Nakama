import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import type { LearnerProgress } from "../../../../modules/formations/domain/Formation";
import type { IFormationEnrollmentRepository } from "../../../../modules/formations/domain/IFormationEnrollmentRepository";
import { Services } from "../../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../../shared/infrastructure/injection/useInjection";
import { useChatbots } from "../../../shared/contexts/ChatbotsContext";
import { useFormations } from "../../../shared/contexts/FormationsContext";
import { useUser } from "../../../shared/contexts/UserContext";

const EMPTY_PROGRESS: LearnerProgress = { completedModuleIds: [], examScores: {} };

/** /formacion/:key — the learner's execute/consume view, open to everyone (admins included). */
export function useFormationExecute() {
  const { key } = useParams<{ key: string }>();
  const { formations } = useFormations();
  const { chatbots } = useChatbots();
  const { currentUser } = useUser();
  const enrollmentRepo = useInjection<IFormationEnrollmentRepository>(Services.FormationEnrollmentRepository.id);
  const navigate = useNavigate();

  const c = key ? formations[key] : undefined;
  const [myProgress, setMyProgress] = useState<LearnerProgress>(EMPTY_PROGRESS);

  useEffect(() => {
    if (!c || !currentUser) {
      setMyProgress(EMPTY_PROGRESS);
      return;
    }
    let cancelled = false;
    enrollmentRepo.FindMyProgress(c.id).then((progress) => {
      if (!cancelled) setMyProgress(progress ?? EMPTY_PROGRESS);
    });
    return () => {
      cancelled = true;
    };
  }, [c, currentUser, enrollmentRepo]);

  const pct = c?.progressPct ?? 0;
  const linkedBot = c?.linkedChatbotKey ? chatbots[c.linkedChatbotKey] : undefined;

  function isLocked(idx: number): boolean {
    if (!c || idx === 0) return false;
    const prev = c.modules[idx - 1];
    return prev.gated && !myProgress.completedModuleIds.includes(prev.id);
  }

  return { c, navigate, pct, linkedBot, isLocked, myProgress };
}
