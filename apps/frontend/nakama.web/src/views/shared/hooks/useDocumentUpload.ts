import { useId, useState, type ChangeEvent, type DragEvent } from "react";
import { documentsConfig } from "../../../shared/infrastructure/config/DocumentsConfig";
import { useToast } from "../contexts/ToastContext";

export function useDocumentUpload(onUpload: (file: File) => Promise<void>, onClose: () => void) {
  const { showToast } = useToast();
  const inputId = useId();
  const [file, setFile] = useState<File | null>(null);
  const [uploading, setUploading] = useState(false);
  const [dragging, setDragging] = useState(false);

  function acceptFile(picked: File) {
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

  function handleFile(e: ChangeEvent<HTMLInputElement>) {
    const picked = e.target.files?.[0];
    e.target.value = "";
    if (picked) acceptFile(picked);
  }

  function handleDragOver(e: DragEvent<HTMLLabelElement>) {
    e.preventDefault();
    setDragging(true);
  }

  function handleDragLeave() {
    setDragging(false);
  }

  function handleDrop(e: DragEvent<HTMLLabelElement>) {
    e.preventDefault();
    setDragging(false);
    const dropped = e.dataTransfer.files[0];
    if (dropped) acceptFile(dropped);
  }

  function handleClose() {
    setFile(null);
    setDragging(false);
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

  return { inputId, file, uploading, dragging, handleFile, handleDragOver, handleDragLeave, handleDrop, handleClose, handleUpload };
}
