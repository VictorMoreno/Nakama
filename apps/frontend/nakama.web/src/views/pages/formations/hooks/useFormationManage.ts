import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import type { FormationLifecycleStatus, LearnerProgress } from "../../../../modules/formations/domain/Formation";
import type { IFormationEnrollmentRepository } from "../../../../modules/formations/domain/IFormationEnrollmentRepository";
import { Services } from "../../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../../shared/infrastructure/injection/useInjection";
import { useFormations } from "../../../shared/contexts/FormationsContext";
import { useToast } from "../../../shared/contexts/ToastContext";

type TabKey = "general" | "docs" | "permisos";

export const FORMATION_TABS: { key: TabKey; label: string }[] = [
  { key: "general", label: "General" },
  { key: "docs", label: "Documentos" },
  { key: "permisos", label: "Permisos" }
];

export function useFormationManage() {
  const { key } = useParams<{ key: string }>();
  const { formations, updateStatus, updateIcon, removeModule, removeTeamMember } = useFormations();
  const { showToast } = useToast();
  const navigate = useNavigate();
  const enrollmentRepo = useInjection<IFormationEnrollmentRepository>(Services.FormationEnrollmentRepository.id);

  const [activeTab, setActiveTab] = useState<TabKey>("general");
  const [expandedModuleId, setExpandedModuleId] = useState<string | null>(null);
  const [creatingNew, setCreatingNew] = useState(false);
  const [deleteId, setDeleteId] = useState<string | null>(null);
  const [addTeamMemberOpen, setAddTeamMemberOpen] = useState(false);
  const [removeTeamMemberUserId, setRemoveTeamMemberUserId] = useState<string | null>(null);
  const [expandedMemberId, setExpandedMemberId] = useState<string | null>(null);
  const [teamProgressResult, setTeamProgressResult] = useState<Record<string, LearnerProgress> | "failed" | null>(null);

  const c = key ? formations[key] : undefined;

  useEffect(() => {
    if (!c || activeTab !== "permisos") return;
    let cancelled = false;
    enrollmentRepo
      .FindTeamProgress(c.id)
      .then((progress) => {
        if (!cancelled) setTeamProgressResult(progress);
      })
      .catch(() => {
        if (!cancelled) setTeamProgressResult("failed");
      });
    return () => {
      cancelled = true;
    };
  }, [c, activeTab, enrollmentRepo]);
  const deleteTarget = c?.modules.find((m) => m.id === deleteId);
  const teamMemberTarget = removeTeamMemberUserId != null ? c?.team.find((m) => m.userId === removeTeamMemberUserId) : undefined;
  const avgTeamProgress = c?.team.length ? Math.round(c.team.reduce((sum, m) => sum + m.progress, 0) / c.team.length) : 0;

  function toggleModuleExpanded(moduleId: string) {
    setCreatingNew(false);
    setExpandedModuleId((prev) => (prev === moduleId ? null : moduleId));
  }

  function toggleMemberExpanded(userId: string) {
    setExpandedMemberId((prev) => (prev === userId ? null : userId));
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
    if (c && removeTeamMemberUserId != null) {
      await removeTeamMember(c.id, removeTeamMemberUserId);
      showToast("Persona quitada de la formación");
    }
    setRemoveTeamMemberUserId(null);
  }

  return {
    c,
    navigate,
    activeTab,
    setActiveTab,
    expandedModuleId,
    toggleModuleExpanded,
    creatingNew,
    startCreating,
    setCreatingNew,
    deleteTarget,
    setDeleteId,
    avgTeamProgress,
    expandedMemberId,
    toggleMemberExpanded,
    teamProgress: teamProgressResult === "failed" ? null : teamProgressResult,
    teamProgressFailed: teamProgressResult === "failed",
    handleUpdateStatus,
    handleUpdateIcon,
    confirmDeleteModule,
    addTeamMemberOpen,
    setAddTeamMemberOpen,
    teamMemberTarget,
    setRemoveTeamMemberUserId,
    confirmRemoveTeamMember
  };
}
