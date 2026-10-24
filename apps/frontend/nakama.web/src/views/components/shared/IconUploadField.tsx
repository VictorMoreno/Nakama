import { useId, type ChangeEvent } from "react";
import { useToast } from "../../shared/contexts/ToastContext";
import { iconsConfig } from "../../../shared/infrastructure/config/IconsConfig";
import { TrashIcon, UploadIcon } from "./icons";
import styles from "./IconUploadField.module.scss";

const FORMATS_LABEL = iconsConfig.allowedContentTypes.map((type) => type.split("/")[1].toUpperCase()).join(", ");

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
    if (!iconsConfig.allowedContentTypes.includes(file.type)) {
      showToast(`Selecciona una imagen (${FORMATS_LABEL})`);
      return;
    }
    if (file.size > iconsConfig.maxSizeMb * 1024 * 1024) {
      showToast(`La imagen no puede superar ${iconsConfig.maxSizeMb} MB`);
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
          <input id={inputId} type="file" accept={iconsConfig.allowedContentTypes.join(",")} onChange={handleFile} hidden />
          {value && (
            <button type="button" className="btn-ghost" onClick={() => onChange(undefined)}>
              <TrashIcon /> Quitar
            </button>
          )}
        </div>
        <span className={styles.iconUploadHint}>{FORMATS_LABEL}, máx. {iconsConfig.maxSizeMb} MB. Se usará como miniatura en la card del listado.</span>
      </div>
    </div>
  );
}
