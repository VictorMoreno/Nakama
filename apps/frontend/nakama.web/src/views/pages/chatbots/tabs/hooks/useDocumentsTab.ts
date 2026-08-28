import { useState } from "react";
import { useChatbots } from "../../../../shared/contexts/ChatbotsContext";
import { useToast } from "../../../../shared/contexts/ToastContext";
import type { Chatbot } from "../../../../../modules/chatbots/domain/Chatbot";

export function useDocumentsTab(bot: Chatbot) {
  const { removeDocument } = useChatbots();
  const { showToast } = useToast();
  const [uploadOpen, setUploadOpen] = useState(false);
  const [deleteIndex, setDeleteIndex] = useState<number | null>(null);
  const target = deleteIndex != null ? bot.docs[deleteIndex] : null;

  function handleRemoveConfirm() {
    if (deleteIndex != null) {
      removeDocument(bot.key, deleteIndex);
      showToast("Documento eliminado");
    }
    setDeleteIndex(null);
  }

  return { uploadOpen, setUploadOpen, deleteIndex, setDeleteIndex, target, handleRemoveConfirm };
}
