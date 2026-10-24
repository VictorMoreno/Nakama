import { useState } from "react";
import { useChatbots } from "../../../../shared/contexts/ChatbotsContext";
import { useToast } from "../../../../shared/contexts/ToastContext";
import type { Chatbot, ChatbotDocument } from "../../../../../modules/chatbots/domain/Chatbot";
import type { IChatbotsRepository } from "../../../../../modules/chatbots/domain/IChatbotsRepository";
import { openDocumentFile } from "../../../../../shared/infrastructure/openDocumentFile";
import { Services } from "../../../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../../../shared/infrastructure/injection/useInjection";

export function useDocumentsTab(bot: Chatbot) {
  const { removeDocument } = useChatbots();
  const { showToast } = useToast();
  const repo = useInjection<IChatbotsRepository>(Services.ChatbotsRepository.id);
  const [uploadOpen, setUploadOpen] = useState(false);
  const [deleteId, setDeleteId] = useState<string | null>(null);
  const target = deleteId != null ? bot.docs.find((doc) => doc.id === deleteId) : null;

  async function handleRemoveConfirm() {
    const documentId = deleteId;
    setDeleteId(null);
    if (documentId == null) return;
    try {
      await removeDocument(bot.id, documentId);
      showToast("Documento eliminado");
    } catch {
      showToast("No se pudo eliminar el documento");
    }
  }

  async function openDocument(doc: ChatbotDocument) {
    if (!doc.id) return;

    try {
      await openDocumentFile(() => repo.DownloadDocument(bot.id, doc.id));
    } catch {
      showToast("No se pudo abrir el documento");
    }
  }

  return { uploadOpen, setUploadOpen, deleteId, setDeleteId, target, handleRemoveConfirm, openDocument };
}
