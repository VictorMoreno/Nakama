import { useState } from "react";
import type { Formation } from "../../../modules/formations/domain/Formation";
import { useFormations } from "../../shared/contexts/FormationsContext";
import { useToast } from "../../shared/contexts/ToastContext";

interface FormationDetailsPanelProps {
  formation: Formation;
}

export default function FormationDetailsPanel({ formation }: FormationDetailsPanelProps) {
  const { updateDetails } = useFormations();
  const { showToast } = useToast();
  const [title, setTitle] = useState(formation.title);
  const [audience, setAudience] = useState(formation.audience);
  const [description, setDescription] = useState(formation.description);
  const [titleError, setTitleError] = useState(false);
  const [audienceError, setAudienceError] = useState(false);
  const [descriptionError, setDescriptionError] = useState(false);
  const [saving, setSaving] = useState(false);

  const dirty =
    title.trim() !== formation.title || audience.trim() !== formation.audience || description.trim() !== formation.description;

  async function handleSave() {
    const nextTitle = title.trim();
    const nextAudience = audience.trim();
    const nextDescription = description.trim();
    if (!nextTitle || !nextAudience || !nextDescription) {
      setTitleError(!nextTitle);
      setAudienceError(!nextAudience);
      setDescriptionError(!nextDescription);
      return;
    }
    setSaving(true);
    try {
      await updateDetails(formation.id, nextTitle, nextAudience, nextDescription);
      showToast("Datos de la formación actualizados");
    } catch {
      showToast("No se pudieron guardar los datos de la formación");
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className="glass" style={{ marginBottom: 14 }}>
      <div className="panel-head">
        <h2>Datos básicos</h2>
      </div>
      <div style={{ padding: "0 20px 20px" }}>
        <div className="form-field">
          <label htmlFor="formation-title">Título</label>
          <input
            id="formation-title"
            type="text"
            className={`form-input${titleError ? " field-error" : ""}`}
            value={title}
            onChange={(e) => {
              setTitle(e.target.value);
              if (titleError) setTitleError(false);
            }}
          />
        </div>
        <div className="form-field">
          <label htmlFor="formation-audience">Audiencia</label>
          <input
            id="formation-audience"
            type="text"
            className={`form-input${audienceError ? " field-error" : ""}`}
            value={audience}
            onChange={(e) => {
              setAudience(e.target.value);
              if (audienceError) setAudienceError(false);
            }}
          />
        </div>
        <div className="form-field">
          <label htmlFor="formation-description">Descripción breve</label>
          <textarea
            id="formation-description"
            className={`form-input${descriptionError ? " field-error" : ""}`}
            rows={2}
            value={description}
            onChange={(e) => {
              setDescription(e.target.value);
              if (descriptionError) setDescriptionError(false);
            }}
          />
        </div>
        <div style={{ display: "flex", justifyContent: "flex-end" }}>
          <button className="btn-primary" type="button" disabled={!dirty || saving} onClick={handleSave}>
            Guardar cambios
          </button>
        </div>
      </div>
    </div>
  );
}
