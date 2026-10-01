import { useId, type ChangeEvent } from "react";
import { useToast } from "../../shared/contexts/ToastContext";
import { RecycleBinIcon, UploadIcon } from "./icons";
import styles from "./IconUploadField.module.scss";

const MAX_BYTES = 2 * 1024 * 1024;

interface IconUploadFieldProps {
  value?: string;
  onChange: (dataUrl: string | undefined) => void;
  /** Used for the fallback initial shown while no image has been chosen — pass the name/title being typed. */
  placeholderLabel: string;
}

export default function IconUploadField({ value, onChange, placeholderLabel }: IconUploadFieldProps) {
  const { showToast } = useToast();
  const inputId = useId();

  function handleFile(e: ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    e.target.value = "";
    if (!file) return;
    if (!file.type.startsWith("image/")) {
      showToast("Selecciona un archivo de imagen");
      return;
    }
    if (file.size > MAX_BYTES) {
      showToast("La imagen no puede superar 2 MB");
      return;
    }
    const reader = new FileReader();
    reader.onerror = () => showToast("No se pudo leer la imagen");
    reader.onload = () => {
      if (typeof reader.result === "string") onChange(reader.result);
    };
    reader.readAsDataURL(file);
  }

  const initial = placeholderLabel.trim().charAt(0).toUpperCase();

  return (
    <div className={styles.iconUpload}>
      <div className={styles.iconUploadPreview} style={value ? { backgroundImage: `url(${value})` } : undefined}>
        {!value && <span>{initial || "?"}</span>}
      </div>
      <div className={styles.iconUploadActions}>
        <div className={styles.iconUploadRow}>
          <label className={`btn-ghost ${styles.iconUploadBtn}`} htmlFor={inputId}>
            <UploadIcon /> {value ? "Cambiar imagen" : "Subir imagen"}
          </label>
          <input id={inputId} type="file" accept="image/*" onChange={handleFile} hidden />
          {value && (
            <button type="button" className="btn-ghost" onClick={() => onChange(undefined)}>
              <RecycleBinIcon /> Quitar
            </button>
          )}
        </div>
        <span className={styles.iconUploadHint}>PNG o JPG, máx. 2 MB. Se usará como miniatura en la card del listado.</span>
      </div>
    </div>
  );
}
