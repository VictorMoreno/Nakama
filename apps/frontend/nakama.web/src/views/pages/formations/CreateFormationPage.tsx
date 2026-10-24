import { useNavigate } from "react-router-dom";
import IconUploadField from "../../components/shared/IconUploadField";
import StatusSelect, { type StatusSelectOption } from "../../components/shared/StatusSelect";
import { ChevronLeftIcon, CloseIcon, PlusIcon } from "../../components/shared/icons";
import { FORMATION_STATUS_META } from "../../../lib/status";
import { useCreateFormation } from "./hooks/useCreateFormation";
import type { FormationLifecycleStatus } from "../../../modules/formations/domain/Formation";
import styles from "./CreateFormationPage.module.scss";

const STATUS_OPTIONS: StatusSelectOption<FormationLifecycleStatus>[] = (
  Object.keys(FORMATION_STATUS_META) as FormationLifecycleStatus[]
).map((value) => ({ value, label: FORMATION_STATUS_META[value].label, level: FORMATION_STATUS_META[value].level }));

export default function CreateFormationPage() {
  const navigate = useNavigate();
  const {
    title,
    setTitle,
    audience,
    setAudience,
    description,
    setDescription,
    status,
    setStatus,
    iconUrl,
    setIconUrl,
    titleError,
    setTitleError,
    audienceError,
    setAudienceError,
    descriptionError,
    setDescriptionError,
    handleConfirm
  } = useCreateFormation();

  return (
    <section className="view active">
      <div className={styles.botDetail}>
        <button className={`btn-ghost ${styles.backBtn}`} type="button" onClick={() => navigate("/formacion")}>
          <ChevronLeftIcon /> Formación
        </button>

        <div className={styles.botDetailHead}>
          <div>
            <h2>Crear formación</h2>
            <p>Podrás añadir módulos y asignar equipo en cuanto lo crees.</p>
          </div>
          <StatusSelect value={status} onChange={setStatus} options={STATUS_OPTIONS} ariaLabel="Estado inicial de la formación" />
        </div>

        <div className="glass">
          <div className="panel-head">
            <h2>Datos básicos</h2>
          </div>
          <div style={{ padding: "0 20px 20px" }}>
            <IconUploadField value={iconUrl} onChange={setIconUrl} placeholderLabel={title || "Formación"} />
            <div className="form-field">
              <label htmlFor="new-formation-title">Título</label>
              <input
                id="new-formation-title"
                type="text"
                className={`form-input${titleError ? " field-error" : ""}`}
                placeholder="p. ej. Onboarding Plataforma Pagos"
                value={title}
                onChange={(e) => {
                  setTitle(e.target.value);
                  if (titleError) setTitleError(false);
                }}
                autoFocus
              />
            </div>
            <div className="form-field">
              <label htmlFor="new-formation-audience">Audiencia</label>
              <input
                id="new-formation-audience"
                type="text"
                className={`form-input${audienceError ? " field-error" : ""}`}
                placeholder="p. ej. Plataforma Pagos, o Toda la empresa"
                value={audience}
                onChange={(e) => {
                  setAudience(e.target.value);
                  if (audienceError) setAudienceError(false);
                }}
              />
            </div>
            <div className="form-field" style={{ marginBottom: 0 }}>
              <label htmlFor="new-formation-desc">Descripción breve</label>
              <textarea
                id="new-formation-desc"
                className={`form-input${descriptionError ? " field-error" : ""}`}
                rows={2}
                placeholder="Se mostrará en la card del listado para ayudar a encontrar esta formación."
                value={description}
                onChange={(e) => {
                  setDescription(e.target.value);
                  if (descriptionError) setDescriptionError(false);
                }}
              />
            </div>
          </div>
        </div>

        <div style={{ display: "flex", justifyContent: "flex-end", gap: 8 }}>
          <button className="btn-ghost" type="button" onClick={() => navigate("/formacion")}>
            <CloseIcon /> Cancelar
          </button>
          <button className="btn-primary" type="button" onClick={handleConfirm}>
            <PlusIcon /> Crear formación
          </button>
        </div>
      </div>
    </section>
  );
}
