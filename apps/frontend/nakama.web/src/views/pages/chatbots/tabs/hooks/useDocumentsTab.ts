import { useState } from "react";
import { useChatbots } from "../../../../shared/contexts/ChatbotsContext";
import { useToast } from "../../../../shared/contexts/ToastContext";
import type { Chatbot, ChatbotDocument, DocumentFile } from "../../../../../modules/chatbots/domain/Chatbot";
import type { IChatbotsRepository } from "../../../../../modules/chatbots/domain/IChatbotsRepository";
import { Services } from "../../../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../../../shared/infrastructure/injection/useInjection";

const BLOB_URL_LIFETIME_MS = 60_000;
const VIEWABLE_TYPES = ["application/pdf", "text/plain"];

function isViewableInBrowser(file: DocumentFile): boolean {
  return VIEWABLE_TYPES.some((type) => file.blob.type.startsWith(type));
}

function saveAsFile(objectUrl: string, fileName: string): void {
  const link: HTMLAnchorElement = document.createElement("a");
  link.href = objectUrl;
  link.download = fileName;
  link.click();
}

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

    const viewer: Window | null = window.open("", "_blank");
    try {
      const file: DocumentFile = await repo.DownloadDocument(bot.id, doc.id);
      const objectUrl: string = URL.createObjectURL(file.blob);

      if (viewer && isViewableInBrowser(file)) {
        viewer.location.href = objectUrl;
      } else {
        viewer?.close();
        saveAsFile(objectUrl, file.fileName);
      }

      setTimeout(() => URL.revokeObjectURL(objectUrl), BLOB_URL_LIFETIME_MS);
    } catch {
      viewer?.close();
      showToast("No se pudo abrir el documento");
    }
  }

  return { uploadOpen, setUploadOpen, deleteId, setDeleteId, target, handleRemoveConfirm, openDocument };
}
