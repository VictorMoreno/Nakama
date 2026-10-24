import { useState } from "react";
import type { Formation, FormationDocument } from "../../../../modules/formations/domain/Formation";
import type { IFormationsRepository } from "../../../../modules/formations/domain/IFormationsRepository";
import { openDocumentFile } from "../../../../shared/infrastructure/openDocumentFile";
import { Services } from "../../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../../shared/infrastructure/injection/useInjection";
import { useFormations } from "../../../shared/contexts/FormationsContext";
import { useToast } from "../../../shared/contexts/ToastContext";

export function useFormationDocuments(formation: Formation) {
  const { addDocument, removeDocument } = useFormations();
  const { showToast } = useToast();
  const repo = useInjection<IFormationsRepository>(Services.FormationsRepository.id);
  const [uploadOpen, setUploadOpen] = useState(false);
  const [deleteId, setDeleteId] = useState<string | null>(null);
  const target = deleteId != null ? formation.docs.find((doc) => doc.id === deleteId) : null;

  async function handleUpload(file: File) {
    await addDocument(formation.id, file, file.name, "Subida manual");
  }

  async function handleRemoveConfirm() {
    const documentId = deleteId;
    setDeleteId(null);
    if (documentId == null) return;
    try {
      await removeDocument(formation.id, documentId);
      showToast("Documento eliminado");
    } catch {
      showToast("No se pudo eliminar el documento");
    }
  }

  async function openDocument(doc: FormationDocument) {
    try {
      await openDocumentFile(() => repo.DownloadDocument(formation.id, doc.id));
    } catch {
      showToast("No se pudo abrir el documento");
    }
  }

  return { uploadOpen, setUploadOpen, setDeleteId, target, handleUpload, handleRemoveConfirm, openDocument };
}
