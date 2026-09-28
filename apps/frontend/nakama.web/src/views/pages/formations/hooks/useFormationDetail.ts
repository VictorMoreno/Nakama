import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import type { FormationLifecycleStatus } from "../../../../modules/formations/domain/Formation";
import { useChatbots } from "../../../shared/contexts/ChatbotsContext";
import { useFormations } from "../../../shared/contexts/FormationsContext";
import { useToast } from "../../../shared/contexts/ToastContext";
import { useUser } from "../../../shared/contexts/UserContext";
import { getFormationProgressPct } from "../../../../lib/status";

export function useFormationDetail() {
  const { key } = useParams<{ key: string }>();
  const { formations, updateStatus, updateIcon, removeModule } = useFormations();
  const { chatbots } = useChatbots();
  const { isAdmin } = useUser();
  const { showToast } = useToast();
  const navigate = useNavigate();
  const [createOpen, setCreateOpen] = useState(false);
  const [deleteId, setDeleteId] = useState<string | null>(null);

  const c = key ? formations[key] : undefined;
  const deleteTarget = c?.modules.find((m) => m.id === deleteId);

  const pct = c ? getFormationProgressPct(c) : 0;
  const linkedBot = c?.linkedChatbotKey ? chatbots[c.linkedChatbotKey] : undefined;
  const avgTeamProgress = c?.team.length ? Math.round(c.team.reduce((sum, m) => sum + m.progress, 0) / c.team.length) : 0;

  function isLocked(idx: number): boolean {
    if (!c || idx === 0) return false;
    const prev = c.modules[idx - 1];
    return prev.gated && !c.myProgress.completedModuleIds.includes(prev.id);
  }

  async function handleUpdateStatus(status: FormationLifecycleStatus) {
    if (c) await updateStatus(c.id, status);
  }

  async function handleUpdateIcon(iconUrl: string | undefined) {
    if (c) await updateIcon(c.id, iconUrl);
  }

  async function confirmDeleteModule() {
    if (c && deleteId) {
      await removeModule(c.id, deleteId);
      showToast("Módulo eliminado");
    }
    setDeleteId(null);
  }

  return {
    c,
    isAdmin,
    navigate,
    createOpen,
    setCreateOpen,
    deleteTarget,
    setDeleteId,
    pct,
    linkedBot,
    avgTeamProgress,
    isLocked,
    handleUpdateStatus,
    handleUpdateIcon,
    confirmDeleteModule
  };
}
