import { useCallback, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useChatbots } from "../../../shared/contexts/ChatbotsContext";
import { useToast } from "../../../shared/contexts/ToastContext";
import { useUser } from "../../../shared/contexts/UserContext";
import { usePagedList } from "../../../shared/hooks/usePagedList";
import { CHATBOT_STATUS_META } from "../../../../lib/status";
import type { ChatbotLifecycleStatus } from "../../../../modules/chatbots/domain/Chatbot";
import type { IChatbotsRepository } from "../../../../modules/chatbots/domain/IChatbotsRepository";
import type { PageQuery } from "../../../../shared/domain/Page";
import { Services } from "../../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../../shared/infrastructure/injection/useInjection";

export type StatusFilter = "all" | ChatbotLifecycleStatus;

const STATUSES = Object.keys(CHATBOT_STATUS_META) as ChatbotLifecycleStatus[];

export const STATUS_OPTIONS: { value: StatusFilter; label: string }[] = [
  { value: "all", label: "Todos los estados" },
  ...STATUSES.map((value) => ({
    value,
    label: CHATBOT_STATUS_META[value].label
  }))
];

export function useChatbotsList() {
  const repo = useInjection<IChatbotsRepository>(Services.ChatbotsRepository.id);
  const { archiveChatbot, unarchiveChatbot } = useChatbots();
  const { isAdmin } = useUser();
  const { showToast } = useToast();
  const navigate = useNavigate();

  const fetchPage = useCallback((query: PageQuery<ChatbotLifecycleStatus>) => repo.FindPage(query), [repo]);
  const list = usePagedList(fetchPage, STATUSES);

  const [deleteKey, setDeleteKey] = useState<string | null>(null);
  const deleteTarget = list.items.find((b) => b.id === deleteKey);

  async function handleDeleteConfirm() {
    const key = deleteKey;
    setDeleteKey(null);
    if (!key) return;
    try {
      await archiveChatbot(key);
      showToast("Chatbot eliminado");
      list.reload();
    } catch {
      showToast("No se pudo eliminar el chatbot — inténtalo de nuevo");
    }
  }

  async function handleRestore(key: string) {
    try {
      await unarchiveChatbot(key);
      showToast("Chatbot reactivado");
      list.reload();
    } catch {
      showToast("No se pudo reactivar el chatbot — inténtalo de nuevo");
    }
  }

  return {
    navigate,
    isAdmin,
    showingArchived: isAdmin && list.archived,
    setShowingArchived: list.setArchived,
    handleRestore,
    chatbots: list.items,
    slide: list.slide,
    displayedPage: list.displayedPage,
    isLoading: list.isLoading,
    search: list.search,
    setSearch: list.setSearch,
    statusFilter: list.status,
    setStatusFilter: list.setStatus,
    pagination: {
      page: list.page,
      pageSize: list.pageSize,
      totalCount: list.totalCount,
      totalPages: list.totalPages,
      onPageChange: list.setPage,
      onPageSizeChange: list.setPageSize
    },
    deleteKey,
    setDeleteKey,
    deleteTarget,
    handleDeleteConfirm
  };
}
