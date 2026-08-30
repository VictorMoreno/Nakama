import { useState } from "react";
import Dialog from "../../components/shared/Dialog";
import Select from "../../components/shared/Select";
import { CloseIcon, PlusIcon } from "../../components/shared/icons";
import { useFormations } from "../../shared/contexts/FormationsContext";
import { useToast } from "../../shared/contexts/ToastContext";
import type { ModuleType } from "../../../modules/formations/domain/Formation";

interface CreateModuleDialogProps {
  open: boolean;
  formationKey: string;
  onClose: () => void;
}

const TYPE_OPTIONS: { value: ModuleType; label: string }[] = [
  { value: "video", label: "Vídeo" },
  { value: "documento", label: "Documento" },
  { value: "ejercicio", label: "Ejercicio" },
  { value: "examen", label: "Examen" }
];

const SUMMARY_LABEL: Record<ModuleType, string> = {
  video: "Resumen del vídeo",
  documento: "Resumen del documento",
  ejercicio: "Instrucciones del ejercicio",
  examen: "Qué valida este examen"
};

export default function CreateModuleDialog({ open, formationKey, onClose }: CreateModuleDialogProps) {
  const [title, setTitle] = useState("");
  const [type, setType] = useState<ModuleType>("documento");
  const [summary, setSummary] = useState("");
  const [resourceUrl, setResourceUrl] = useState("");
  const [durationMin, setDurationMin] = useState("10");
  const [passingScore, setPassingScore] = useState("70");
  const [gated, setGated] = useState(true);
  const [titleError, setTitleError] = useState(false);
  const [summaryError, setSummaryError] = useState(false);
  const [urlError, setUrlError] = useState(false);
  const { addModule } = useFormations();
  const { showToast } = useToast();

  function reset() {
    setTitle("");
    setType("documento");
    setSummary("");
    setResourceUrl("");
    setDurationMin("10");
    setPassingScore("70");
    setGated(true);
    setTitleError(false);
    setSummaryError(false);
    setUrlError(false);
  }

  function handleClose() {
    reset();
    onClose();
  }

  function handleConfirm() {
    const trimmedTitle = title.trim();
    const trimmedSummary = summary.trim();
    const needsUrl = type === "video" || type === "documento" || type === "ejercicio";
    const trimmedUrl = resourceUrl.trim();

    setTitleError(!trimmedTitle);
    setSummaryError(!trimmedSummary);
    setUrlError(needsUrl && !trimmedUrl);
    if (!trimmedTitle || !trimmedSummary || (needsUrl && !trimmedUrl)) return;

    if (type === "examen") {
      addModule(formationKey, {
        title: trimmedTitle,
        type,
        summary: trimmedSummary,
        gated,
        aiDraft: true,
        passingScore: Number(passingScore) || 70,
        quiz: [
          {
            question: `Pregunta generada automáticamente sobre "${trimmedTitle}" a partir de los documentos indexados`,
            options: ["Opción A", "Opción B (correcta)", "Opción C", "Opción D"],
            correctIndex: 1
          }
        ]
      });
      showToast("Módulo de examen creado — preguntas generadas por IA, pendientes de revisión");
    } else {
      addModule(formationKey, {
        title: trimmedTitle,
        type,
        summary: trimmedSummary,
        gated,
        aiDraft: false,
        resourceUrl: trimmedUrl,
        durationMin: type === "video" ? Number(durationMin) || undefined : undefined
      });
      showToast("Módulo creado");
    }
    reset();
    onClose();
  }

  return (
    <Dialog open={open} onClose={handleClose} title="Nuevo módulo de formación">
      <div className="form-field">
        <label htmlFor="module-title">Título</label>
        <input
          id="module-title"
          type="text"
          className={`form-input${titleError ? " field-error" : ""}`}
          placeholder="p. ej. Vídeo: primeros pasos con el repositorio"
          value={title}
          onChange={(e) => {
            setTitle(e.target.value);
            if (titleError) setTitleError(false);
          }}
          autoFocus
        />
      </div>

      <div className="form-field">
        <label htmlFor="module-type">Tipo de paso</label>
        <Select id="module-type" value={type} onChange={setType} options={TYPE_OPTIONS} ariaLabel="Tipo de paso" fullWidth />
      </div>

      <div className="form-field">
        <label htmlFor="module-summary">{SUMMARY_LABEL[type]}</label>
        <textarea
          id="module-summary"
          className={`form-input${summaryError ? " field-error" : ""}`}
          rows={3}
          value={summary}
          onChange={(e) => {
            setSummary(e.target.value);
            if (summaryError) setSummaryError(false);
          }}
        />
      </div>

      {(type === "video" || type === "documento" || type === "ejercicio") && (
        <div className="form-field">
          <label htmlFor="module-url">
            {type === "video" ? "Enlace al vídeo" : type === "documento" ? "Enlace al documento" : "Enlace al ejercicio/repositorio"}
          </label>
          <input
            id="module-url"
            type="text"
            className={`form-input${urlError ? " field-error" : ""}`}
            placeholder="https://…"
            value={resourceUrl}
            onChange={(e) => {
              setResourceUrl(e.target.value);
              if (urlError) setUrlError(false);
            }}
          />
        </div>
      )}

      {type === "video" && (
        <div className="form-field">
          <label htmlFor="module-duration">Duración (min)</label>
          <input
            id="module-duration"
            type="number"
            min={1}
            className="form-input"
            value={durationMin}
            onChange={(e) => setDurationMin(e.target.value)}
          />
        </div>
      )}

      {type === "examen" && (
        <>
          <div className="form-field">
            <label htmlFor="module-score">Nota mínima para aprobar (%)</label>
            <input
              id="module-score"
              type="number"
              min={0}
              max={100}
              className="form-input"
              value={passingScore}
              onChange={(e) => setPassingScore(e.target.value)}
            />
          </div>
          <p className="form-hint">
            Las preguntas se generan automáticamente a partir de los documentos indexados del chatbot de referencia — quedan
            como borrador pendiente de revisión antes de publicarse.
          </p>
        </>
      )}

      <div className="config-row" style={{ padding: "10px 0", border: "none" }}>
        <div className="config-row-text">
          <div className="title">Bloquea el siguiente módulo</div>
          <div className="desc">Los juniors no podrán acceder al siguiente módulo hasta completar este.</div>
        </div>
        <button className="switch" role="switch" aria-checked={gated} aria-label="Bloquea el siguiente módulo" type="button" onClick={() => setGated((g) => !g)} />
      </div>

      <div className="modal-actions">
        <button className="btn-ghost" type="button" onClick={handleClose}>
          <CloseIcon /> Cancelar
        </button>
        <button className="btn-primary" type="button" onClick={handleConfirm}>
          <PlusIcon /> Crear módulo
        </button>
      </div>
    </Dialog>
  );
}
