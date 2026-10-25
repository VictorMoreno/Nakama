import { type ReactNode } from "react";
import Dialog from "./Dialog";
import { CloseIcon, UploadIcon } from "./icons";
import { documentsConfig } from "../../../shared/infrastructure/config/DocumentsConfig";
import { useDocumentUpload } from "../../shared/hooks/useDocumentUpload";

interface DocumentUploadDialogProps {
  open: boolean;
  description: ReactNode;
  onUpload: (file: File) => Promise<void>;
  onClose: () => void;
}

export default function DocumentUploadDialog({ open, description, onUpload, onClose }: DocumentUploadDialogProps) {
  const { inputId, file, uploading, dragging, handleFile, handleDragOver, handleDragLeave, handleDrop, handleClose, handleUpload } =
    useDocumentUpload(onUpload, onClose);

  return (
    <Dialog open={open} onClose={handleClose} title="Subir documento">
      <p>{description}</p>
      <label
        className={`dropzone${dragging ? " dragging" : ""}`}
        htmlFor={inputId}
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
        onDrop={handleDrop}
      >
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
