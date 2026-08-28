import IconUploadField from "../../components/shared/IconUploadField";
import Select from "../../components/shared/Select";
import StatusSelect, { type StatusSelectOption } from "../../components/shared/StatusSelect";
import { CHATBOT_STATUS_META } from "../../../lib/status";
import type { ChatbotLifecycleStatus, LlmProvider } from "../../../modules/chatbots/domain/Chatbot";
import { useCreateChatbot } from "./hooks/useCreateChatbot";
import styles from "./CreateChatbotPage.module.scss";

const STATUS_OPTIONS: StatusSelectOption<ChatbotLifecycleStatus>[] = (
  Object.keys(CHATBOT_STATUS_META) as ChatbotLifecycleStatus[]
).map((value) => ({ value, label: CHATBOT_STATUS_META[value].label, level: CHATBOT_STATUS_META[value].level }));

const PROVIDER_OPTIONS: { value: LlmProvider; label: string }[] = [
  { value: "anthropic", label: "Anthropic (Claude)" },
  { value: "openai", label: "OpenAI (GPT)" },
  { value: "local", label: "Modelo local" }
];

export default function CreateChatbotPage() {
  const {
    navigate,
    name,
    setName,
    general,
    setGeneral,
    dept,
    setDept,
    description,
    setDescription,
    status,
    setStatus,
    socratic,
    setSocratic,
    explicaMas,
    setExplicaMas,
    provider,
    setProvider,
    iconUrl,
    setIconUrl,
    nameError,
    setNameError,
    handleConfirm
  } = useCreateChatbot();

  return (
    <section className="view active">
      <div className={styles.botDetail}>
        <button className={`btn-ghost ${styles.backBtn}`} type="button" onClick={() => navigate("/chatbots")}>
          ← Gestión de chatbots
        </button>

        <div className={styles.botDetailHead}>
          <div>
            <h2>Crear chatbot</h2>
            <p>Se te añadirá como propietario. Podrás subir documentos e invitar a tu equipo en cuanto lo crees.</p>
          </div>
          <StatusSelect value={status} onChange={setStatus} options={STATUS_OPTIONS} ariaLabel="Estado inicial del chatbot" />
        </div>

        <div className="glass">
          <div className="panel-head">
            <h2>Datos básicos</h2>
          </div>
          <div style={{ padding: "0 20px 20px" }}>
            <IconUploadField value={iconUrl} onChange={setIconUrl} placeholderLabel={name || "Chatbot"} />
            <div className="form-field">
              <label htmlFor="new-bot-name">Nombre del chatbot</label>
              <input
                id="new-bot-name"
                type="text"
                className={`form-input${nameError ? " field-error" : ""}`}
                placeholder="p. ej. Plataforma Pagos"
                value={name}
                onChange={(e) => {
                  setName(e.target.value);
                  if (nameError) setNameError(false);
                }}
                autoFocus
              />
            </div>
            {!general && (
              <div className="form-field">
                <label htmlFor="new-bot-dept">Departamento o equipo</label>
                <input
                  id="new-bot-dept"
                  type="text"
                  className="form-input"
                  placeholder="p. ej. Finanzas"
                  value={dept}
                  onChange={(e) => setDept(e.target.value)}
                />
              </div>
            )}
            <div className="form-field" style={{ marginBottom: 0 }}>
              <label htmlFor="new-bot-desc">Descripción breve</label>
              <textarea
                id="new-bot-desc"
                className="form-input"
                rows={2}
                placeholder="Se mostrará en la card del listado para ayudar a encontrar este chatbot."
                value={description}
                onChange={(e) => setDescription(e.target.value)}
              />
            </div>
          </div>

          <div className={styles.configRows}>
            <div className={styles.configRow}>
              <div className={styles.configRowText}>
                <div className="title">Chatbot general de empresa</div>
                <div className="desc">
                  En vez de un equipo concreto, responde con información general de la empresa (políticas, RR. HH., IT…).
                </div>
              </div>
              <button
                className="switch"
                role="switch"
                aria-checked={general}
                aria-label="Chatbot general de empresa"
                type="button"
                onClick={() => setGeneral((g) => !g)}
              />
            </div>
          </div>
        </div>

        <div className="glass" style={{ marginTop: 14 }}>
          <div className="panel-head">
            <h2>Comportamiento</h2>
          </div>
          <p style={{ padding: "0 20px 14px", fontSize: "0.8rem", color: "var(--ink-soft)", maxWidth: "60ch" }}>
            Esto se configura una vez para todo el equipo — quien use el chatbot no puede cambiarlo desde el propio chat.
          </p>
          <div className={styles.configRows}>
            <div className={styles.configRow}>
              <div className={styles.configRowText}>
                <div className="title">Modo Socrático</div>
                <div className="desc">El bot guía con preguntas en vez de dar la respuesta directa.</div>
              </div>
              <button
                className="switch"
                role="switch"
                aria-checked={socratic}
                aria-label="Modo Socrático"
                type="button"
                onClick={() => setSocratic((s) => !s)}
              />
            </div>

            <div className={styles.configRow}>
              <div className={styles.configRowText}>
                <div className="title">Explícame más</div>
                <div className="desc">Permite pedir una capa extra de profundidad técnica en cada respuesta.</div>
              </div>
              <button
                className="switch"
                role="switch"
                aria-checked={explicaMas}
                aria-label="Explícame más"
                type="button"
                onClick={() => setExplicaMas((e) => !e)}
              />
            </div>

            <div className={styles.configRow}>
              <div className={styles.configRowText}>
                <div className="title">Modelo de lenguaje</div>
                <div className="desc">Proveedor que responde las preguntas de este chatbot.</div>
              </div>
              <Select value={provider} onChange={setProvider} options={PROVIDER_OPTIONS} ariaLabel="Modelo de lenguaje" />
            </div>
          </div>
        </div>

        <div style={{ display: "flex", justifyContent: "flex-end", gap: 8 }}>
          <button className="btn-ghost" type="button" onClick={() => navigate("/chatbots")}>
            Cancelar
          </button>
          <button className="btn-primary" type="button" onClick={handleConfirm}>
            Crear chatbot
          </button>
        </div>
      </div>
    </section>
  );
}
