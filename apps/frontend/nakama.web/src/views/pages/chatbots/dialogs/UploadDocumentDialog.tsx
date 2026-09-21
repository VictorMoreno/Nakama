import { useId, useState, type ChangeEvent } from "react";
import Dialog from "../../../components/shared/Dialog";
import { CloseIcon, UploadIcon } from "../../../components/shared/icons";
import { useChatbots } from "../../../shared/contexts/ChatbotsContext";
import { useToast } from "../../../shared/contexts/ToastContext";

// Mirrors DocumentStorage's server-side limits (apps/backend/api/Configuration/DocumentStorage.cs)
// - checked here too so a rejected upload never even reaches the network.
const MAX_BYTES = 20 * 1024 * 1024;
const ALLOWED_EXTENSIONS = [".pdf", ".md", ".markdown", ".txt"];

interface UploadDocumentDialogProps {
  open: boolean;
  botId: string;
  botLabel: string;
  onClose: () => void;
}

export default function UploadDocumentDialog({ open, botId, botLabel, onClose }: UploadDocumentDialogProps) {
  const { addDocument } = useChatbots();
  const { showToast } = useToast();
  const inputId = useId();
  const [file, setFile] = useState<File | null>(null);
  const [uploading, setUploading] = useState(false);

  function handleFile(e: ChangeEvent<HTMLInputElement>) {
    const picked = e.target.files?.[0];
    e.target.value = "";
    if (!picked) return;
    const dot = picked.name.lastIndexOf(".");
    const extension = dot >= 0 ? picked.name.slice(dot).toLowerCase() : "";
    if (!ALLOWED_EXTENSIONS.includes(extension)) {
      showToast("Selecciona un PDF, Markdown o archivo de texto");
      return;
    }
    if (picked.size > MAX_BYTES) {
      showToast("El documento no puede superar 20 MB");
      return;
    }
    setFile(picked);
  }

  function handleClose() {
    setFile(null);
    onClose();
  }

  async function handleUpload() {
    if (!file) return;
    setUploading(true);
    try {
      await addDocument(botId, file, file.name, "Subida manual");
      showToast("Documento en cola de indexado");
      handleClose();
    } catch {
      showToast("No se pudo subir el documento");
    } finally {
      setUploading(false);
    }
  }

  return (
    <Dialog open={open} onClose={handleClose} title="Subir documento">
      <p>
        Sube un PDF, Markdown o archivo de texto. Se indexa en <strong>{botLabel}</strong> — solo visible para este chatbot.
      </p>
      <label className="dropzone" htmlFor={inputId} style={{ cursor: "pointer", display: "block" }}>
        {file ? file.name : "Arrastra un archivo aquí, o haz clic para seleccionar"}
      </label>
      <input id={inputId} type="file" accept=".pdf,.md,.markdown,.txt" onChange={handleFile} hidden />
      <div className="modal-actions">
        <button className="btn-ghost" type="button" onClick={handleClose}>
          <CloseIcon /> Cancelar
        </button>
        <button className="btn-primary" type="button" onClick={handleUpload} disabled={!file || uploading}>
          <UploadIcon /> {uploading ? "Subiendo…" : "Subir"}
        </button>
      </div>
    </Dialog>
  );
}
