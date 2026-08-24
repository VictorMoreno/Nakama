import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { IconUploadField } from "../../components/IconUploadField";
import { Select } from "../../components/Select";
import { StatusSelect, type StatusSelectOption } from "../../components/StatusSelect";
import { useChatbots } from "../../context/ChatbotsContext";
import { useCurriculums } from "../../context/CurriculumsContext";
import { useToast } from "../../context/ToastContext";
import { CURRICULUM_STATUS_META } from "../../lib/status";
import type { CurriculumLifecycleStatus } from "../../types/curriculum";

const STATUS_OPTIONS: StatusSelectOption<CurriculumLifecycleStatus>[] = (
  Object.keys(CURRICULUM_STATUS_META) as CurriculumLifecycleStatus[]
).map((value) => ({ value, label: CURRICULUM_STATUS_META[value].label, level: CURRICULUM_STATUS_META[value].level }));

export function CreateCurriculumView() {
  const navigate = useNavigate();
  const { activeChatbots } = useChatbots();
  const { createCurriculum } = useCurriculums();
  const { showToast } = useToast();

  const [title, setTitle] = useState("");
  const [audience, setAudience] = useState("");
  const [description, setDescription] = useState("");
  const [status, setStatus] = useState<CurriculumLifecycleStatus>("borrador");
  const [linkedChatbotKey, setLinkedChatbotKey] = useState("");
  const [iconUrl, setIconUrl] = useState<string | undefined>(undefined);
  const [titleError, setTitleError] = useState(false);

  function handleConfirm() {
    const trimmed = title.trim();
    if (!trimmed) {
      setTitleError(true);
      return;
    }
    const key = createCurriculum({
      title: trimmed,
      audience: audience.trim(),
      description,
      status,
      linkedChatbotKey: linkedChatbotKey || undefined,
      iconUrl
    });
    showToast("Currículo creado");
    navigate(`/formacion/${key}`);
  }

  return (
    <section className="view active">
      <div className="bot-detail">
        <button className="btn-ghost back-btn" type="button" onClick={() => navigate("/formacion")}>
          ← Formación
        </button>

        <div className="bot-detail-head">
          <div>
            <h2>Crear currículo</h2>
            <p>Podrás añadir módulos y asignar equipo en cuanto lo crees.</p>
          </div>
          <StatusSelect value={status} onChange={setStatus} options={STATUS_OPTIONS} ariaLabel="Estado inicial del currículo" />
        </div>

        <div className="glass">
          <div className="panel-head">
            <h2>Datos básicos</h2>
          </div>
          <div style={{ padding: "0 20px 20px" }}>
            <IconUploadField value={iconUrl} onChange={setIconUrl} placeholderLabel={title || "Currículo"} />
            <div className="form-field">
              <label htmlFor="new-curriculum-title">Título</label>
              <input
                id="new-curriculum-title"
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
              <label htmlFor="new-curriculum-audience">Audiencia</label>
              <input
                id="new-curriculum-audience"
                type="text"
                className="form-input"
                placeholder="p. ej. Plataforma Pagos, o Toda la empresa"
                value={audience}
                onChange={(e) => setAudience(e.target.value)}
              />
            </div>
            <div className="form-field" style={{ marginBottom: 0 }}>
              <label htmlFor="new-curriculum-desc">Descripción breve</label>
              <textarea
                id="new-curriculum-desc"
                className="form-input"
                rows={2}
                placeholder="Se mostrará en la card del listado para ayudar a encontrar este currículo."
                value={description}
                onChange={(e) => setDescription(e.target.value)}
              />
            </div>
          </div>

          <div className="config-rows">
            <div className="config-row">
              <div className="config-row-text">
                <div className="title">Chatbot de referencia</div>
                <div className="desc">
                  Opcional — solo se usa para generar borradores de examen con IA a partir de su documentación. El
                  currículo funciona igual sin ninguno asociado.
                </div>
              </div>
              <Select
                id="new-curriculum-chatbot"
                value={linkedChatbotKey}
                onChange={setLinkedChatbotKey}
                options={[{ value: "", label: "Ninguno" }, ...activeChatbots.map((bot) => ({ value: bot.key, label: bot.label }))]}
                ariaLabel="Chatbot de referencia"
              />
            </div>
          </div>
        </div>

        <div style={{ display: "flex", justifyContent: "flex-end", gap: 8 }}>
          <button className="btn-ghost" type="button" onClick={() => navigate("/formacion")}>
            Cancelar
          </button>
          <button className="btn-primary" type="button" onClick={handleConfirm}>
            Crear currículo
          </button>
        </div>
      </div>
    </section>
  );
}
