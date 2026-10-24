import { useId, useState, type ChangeEvent, type ReactNode } from "react";
import Dialog from "./Dialog";
import { CloseIcon, UploadIcon } from "./icons";
import { useToast } from "../../shared/contexts/ToastContext";
import { documentsConfig } from "../../../shared/infrastructure/config/DocumentsConfig";

interface DocumentUploadDialogProps {
  open: boolean;
  description: ReactNode;
  onUpload: (file: File) => Promise<void>;
  onClose: () => void;
}

export default function DocumentUploadDialog({ open, description, onUpload, onClose }: DocumentUploadDialogProps) {
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
    if (!documentsConfig.allowedExtensions.includes(extension)) {
      showToast(`Selecciona un archivo permitido (${documentsConfig.allowedExtensions.join(", ")})`);
      return;
    }
    if (picked.size > documentsConfig.maxSizeMb * 1024 * 1024) {
      showToast(`El documento no puede superar ${documentsConfig.maxSizeMb} MB`);
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
      await onUpload(file);
      showToast("Documento añadido");
      handleClose();
    } catch {
      showToast("No se pudo subir el documento");
    } finally {
      setUploading(false);
    }
  }

  return (
    <Dialog open={open} onClose={handleClose} title="Subir documento">
      <p>{description}</p>
      <label className="dropzone" htmlFor={inputId} style={{ cursor: "pointer", display: "block" }}>
        {file ? file.name : "Arrastra un archivo aquí, o haz clic para seleccionar"}
      </label>
      <input id={inputId} type="file" accept={documentsConfig.allowedExtensions.join(",")} onChange={handleFile} hidden />
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
