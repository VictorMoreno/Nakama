import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useChatbots } from "../../../shared/contexts/ChatbotsContext";
import { useToast } from "../../../shared/contexts/ToastContext";
import { CHATBOT_STATUS_META } from "../../../../lib/status";
import type { ChatbotLifecycleStatus } from "../../../../modules/chatbots/domain/Chatbot";

export type StatusFilter = "all" | ChatbotLifecycleStatus;

export const STATUS_OPTIONS: { value: StatusFilter; label: string }[] = [
  { value: "all", label: "Todos los estados" },
  ...(Object.keys(CHATBOT_STATUS_META) as ChatbotLifecycleStatus[]).map((value) => ({
    value,
    label: CHATBOT_STATUS_META[value].label
  }))
];

export function useChatbotsList() {
  const { activeChatbots, archiveChatbot } = useChatbots();
  const { showToast } = useToast();
  const navigate = useNavigate();

  const [deleteKey, setDeleteKey] = useState<string | null>(null);
  const deleteTarget = activeChatbots.find((b) => b.key === deleteKey);

  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState<StatusFilter>("all");

  const filteredChatbots = activeChatbots.filter((bot) => {
    const query = search.trim().toLowerCase();
    const matchesSearch =
      query.length === 0 || bot.label.toLowerCase().includes(query) || bot.description.toLowerCase().includes(query);
    const matchesStatus = statusFilter === "all" || bot.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  function handleDeleteConfirm() {
    if (deleteKey) {
      archiveChatbot(deleteKey);
      showToast("Chatbot eliminado (demo)");
    }
    setDeleteKey(null);
  }

  return {
    navigate,
    search,
    setSearch,
    statusFilter,
    setStatusFilter,
    filteredChatbots,
    deleteKey,
    setDeleteKey,
    deleteTarget,
    handleDeleteConfirm
  };
}
