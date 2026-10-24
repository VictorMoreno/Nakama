import { useCallback, useState } from "react";
import type { FormationLifecycleStatus } from "../../../../modules/formations/domain/Formation";
import type { IFormationsRepository } from "../../../../modules/formations/domain/IFormationsRepository";
import type { PageQuery } from "../../../../shared/domain/Page";
import { Services } from "../../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../../shared/infrastructure/injection/useInjection";
import { FORMATION_STATUS_META } from "../../../../lib/status";
import { useFormations } from "../../../shared/contexts/FormationsContext";
import { useToast } from "../../../shared/contexts/ToastContext";
import { useUser } from "../../../shared/contexts/UserContext";
import { usePagedList } from "../../../shared/hooks/usePagedList";

export type StatusFilter = "all" | FormationLifecycleStatus;

const STATUSES = Object.keys(FORMATION_STATUS_META) as FormationLifecycleStatus[];

export const STATUS_OPTIONS: { value: StatusFilter; label: string }[] = [
  { value: "all", label: "Todos los estados" },
  ...STATUSES.map((value) => ({
    value,
    label: FORMATION_STATUS_META[value].label
  }))
];

export function useFormationsList() {
  const repo = useInjection<IFormationsRepository>(Services.FormationsRepository.id);
  const { archiveFormation, unarchiveFormation } = useFormations();
  const { isAdmin } = useUser();
  const { showToast } = useToast();

  const fetchPage = useCallback((query: PageQuery<FormationLifecycleStatus>) => repo.FindPage(query), [repo]);
  const list = usePagedList(fetchPage, STATUSES, isAdmin ? undefined : "in_progress");

  const [deleteKey, setDeleteKey] = useState<string | null>(null);
  const deleteTarget = list.items.find((c) => c.id === deleteKey);

  async function confirmDelete() {
    const key = deleteKey;
    setDeleteKey(null);
    if (!key) return;
    try {
      await archiveFormation(key);
      showToast("Formación eliminada");
      list.reload();
    } catch {
      showToast("No se pudo eliminar la formación — inténtalo de nuevo");
    }
  }

  async function handleRestore(key: string) {
    try {
      await unarchiveFormation(key);
      showToast("Formación reactivada");
      list.reload();
    } catch {
      showToast("No se pudo reactivar la formación — inténtalo de nuevo");
    }
  }

  return {
    isAdmin,
    showingArchived: isAdmin && list.archived,
    setShowingArchived: list.setArchived,
    handleRestore,
    formations: list.items,
    slide: list.slide,
    displayedPage: list.displayedPage,
    isLoading: list.isLoading,
    search: list.search,
    setSearch: list.setSearch,
    statusFilter: list.status,
    setStatusFilter: list.setStatus,
    progressFilter: list.progress ?? "in_progress",
    setProgressFilter: list.setProgress,
    pagination: {
      page: list.page,
      pageSize: list.pageSize,
      totalCount: list.totalCount,
      totalPages: list.totalPages,
      onPageChange: list.setPage,
      onPageSizeChange: list.setPageSize
    },
    deleteTarget,
    setDeleteKey,
    confirmDelete
  };
}
