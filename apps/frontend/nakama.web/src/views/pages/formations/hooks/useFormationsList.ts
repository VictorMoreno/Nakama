import { useState } from "react";
import type { FormationLifecycleStatus } from "../../../../modules/formations/domain/Formation";
import { useChatbots } from "../../../shared/contexts/ChatbotsContext";
import { useFormations } from "../../../shared/contexts/FormationsContext";
import { useToast } from "../../../shared/contexts/ToastContext";
import { useUser } from "../../../shared/contexts/UserContext";

export type StatusFilter = "all" | FormationLifecycleStatus;

export function useFormationsList() {
  const { activeFormations, archiveFormation } = useFormations();
  const { chatbots } = useChatbots();
  const { isAdmin } = useUser();
  const { showToast } = useToast();

  const [deleteKey, setDeleteKey] = useState<string | null>(null);
  const deleteTarget = activeFormations.find((c) => c.id === deleteKey);

  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState<StatusFilter>("all");

  const withProgress = activeFormations.map((c) => ({ c, pct: c.progressPct }));

  const filteredFormations = withProgress.filter(({ c }) => {
    const query = search.trim().toLowerCase();
    const matchesSearch =
      query.length === 0 ||
      c.title.toLowerCase().includes(query) ||
      c.audience.toLowerCase().includes(query) ||
      c.description.toLowerCase().includes(query);
    const matchesStatus = statusFilter === "all" || c.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  async function confirmDelete() {
    if (deleteKey) {
      await archiveFormation(deleteKey);
      showToast("Formación eliminada (demo)");
    }
    setDeleteKey(null);
  }

  return {
    chatbots,
    isAdmin,
    search,
    setSearch,
    statusFilter,
    setStatusFilter,
    filteredFormations,
    deleteKey,
    deleteTarget,
    setDeleteKey,
    confirmDelete
  };
}
