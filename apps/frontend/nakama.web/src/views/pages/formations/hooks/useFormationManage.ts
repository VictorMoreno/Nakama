import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import type { FormationLifecycleStatus } from "../../../../modules/formations/domain/Formation";
import { useChatbots } from "../../../shared/contexts/ChatbotsContext";
import { useFormations } from "../../../shared/contexts/FormationsContext";
import { useToast } from "../../../shared/contexts/ToastContext";

/** /formacion/:key/gestionar (RequireAdmin) — the admin curriculum-building view. */
export function useFormationManage() {
  const { key } = useParams<{ key: string }>();
  const { formations, updateStatus, updateIcon, removeModule, removeTeamMember } = useFormations();
  const { chatbots } = useChatbots();
  const { showToast } = useToast();
  const navigate = useNavigate();

  const [expandedModuleId, setExpandedModuleId] = useState<string | null>(null);
  const [creatingNew, setCreatingNew] = useState(false);
  const [deleteId, setDeleteId] = useState<string | null>(null);
  const [addTeamMemberOpen, setAddTeamMemberOpen] = useState(false);
  const [removeTeamMemberIndex, setRemoveTeamMemberIndex] = useState<number | null>(null);

  const c = key ? formations[key] : undefined;
  const deleteTarget = c?.modules.find((m) => m.id === deleteId);
  const teamMemberTarget = removeTeamMemberIndex != null ? c?.team[removeTeamMemberIndex] : undefined;
  const avgTeamProgress = c?.team.length ? Math.round(c.team.reduce((sum, m) => sum + m.progress, 0) / c.team.length) : 0;
  const linkedBot = c?.linkedChatbotKey ? chatbots[c.linkedChatbotKey] : undefined;

  function toggleModuleExpanded(moduleId: string) {
    setCreatingNew(false);
    setExpandedModuleId((prev) => (prev === moduleId ? null : moduleId));
  }

  function startCreating() {
    setExpandedModuleId(null);
    setCreatingNew(true);
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

  async function confirmRemoveTeamMember() {
    if (c && removeTeamMemberIndex != null) {
      await removeTeamMember(c.id, removeTeamMemberIndex);
      showToast("Persona quitada de la formación");
    }
    setRemoveTeamMemberIndex(null);
  }

  return {
    c,
    navigate,
    linkedBot,
    expandedModuleId,
    toggleModuleExpanded,
    creatingNew,
    startCreating,
    setCreatingNew,
    deleteTarget,
    setDeleteId,
    avgTeamProgress,
    handleUpdateStatus,
    handleUpdateIcon,
    confirmDeleteModule,
    addTeamMemberOpen,
    setAddTeamMemberOpen,
    teamMemberTarget,
    setRemoveTeamMemberIndex,
    confirmRemoveTeamMember
  };
}
