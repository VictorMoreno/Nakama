import { useNavigate, useParams } from "react-router-dom";
import { useChatbots } from "../../../shared/contexts/ChatbotsContext";
import { useFormations } from "../../../shared/contexts/FormationsContext";
import { getFormationProgressPct } from "../../../../lib/status";

/** /formacion/:key — the learner's execute/consume view, open to everyone (admins included). */
export function useFormationExecute() {
  const { key } = useParams<{ key: string }>();
  const { formations } = useFormations();
  const { chatbots } = useChatbots();
  const navigate = useNavigate();

  const c = key ? formations[key] : undefined;
  const pct = c ? getFormationProgressPct(c) : 0;
  const linkedBot = c?.linkedChatbotKey ? chatbots[c.linkedChatbotKey] : undefined;

  function isLocked(idx: number): boolean {
    if (!c || idx === 0) return false;
    const prev = c.modules[idx - 1];
    return prev.gated && !c.myProgress.completedModuleIds.includes(prev.id);
  }

  return { c, navigate, pct, linkedBot, isLocked };
}
