import type { Formation } from "../../../modules/formations/domain/Formation";
import { useFormationDetails } from "./hooks/useFormationDetails";

interface FormationDetailsPanelProps {
  formation: Formation;
}

export default function FormationDetailsPanel({ formation }: FormationDetailsPanelProps) {
  const {
    title,
    audience,
    description,
    titleError,
    audienceError,
    descriptionError,
    saving,
    dirty,
    changeTitle,
    changeAudience,
    changeDescription,
    handleSave
  } = useFormationDetails(formation);

  return (
    <div className="glass panel-spaced">
      <div className="panel-head">
        <h2>Datos básicos</h2>
      </div>
      <div className="panel-body">
        <div className="form-field">
          <label htmlFor="formation-title">Título</label>
          <input
            id="formation-title"
            type="text"
            className={`form-input${titleError ? " field-error" : ""}`}
            value={title}
            onChange={(e) => changeTitle(e.target.value)}
          />
        </div>
        <div className="form-field">
          <label htmlFor="formation-audience">Audiencia</label>
          <input
            id="formation-audience"
            type="text"
            className={`form-input${audienceError ? " field-error" : ""}`}
            value={audience}
            onChange={(e) => changeAudience(e.target.value)}
          />
        </div>
        <div className="form-field">
          <label htmlFor="formation-description">Descripción breve</label>
          <textarea
            id="formation-description"
            className={`form-input${descriptionError ? " field-error" : ""}`}
            rows={2}
            value={description}
            onChange={(e) => changeDescription(e.target.value)}
          />
        </div>
        <div className="form-actions">
          <button className="btn-primary" type="button" disabled={!dirty || saving} onClick={handleSave}>
            Guardar cambios
          </button>
        </div>
      </div>
    </div>
  );
}
