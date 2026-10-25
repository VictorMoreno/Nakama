import IconUploadField from "../../components/shared/IconUploadField";
import Select from "../../components/shared/Select";
import StatusSelect, { type StatusSelectOption } from "../../components/shared/StatusSelect";
import { ChevronLeftIcon, CloseIcon, PlusIcon } from "../../components/shared/icons";
import { CHATBOT_STATUS_META } from "../../../lib/status";
import type { ChatbotLifecycleStatus, LlmProvider } from "../../../modules/chatbots/domain/Chatbot";
import { useCreateChatbot } from "./hooks/useCreateChatbot";
import styles from "./CreateChatbotPage.module.scss";

const STATUS_OPTIONS: StatusSelectOption<ChatbotLifecycleStatus>[] = (
  Object.keys(CHATBOT_STATUS_META) as ChatbotLifecycleStatus[]
).map((value) => ({ value, label: CHATBOT_STATUS_META[value].label, level: CHATBOT_STATUS_META[value].level }));

const PROVIDER_OPTIONS: { value: LlmProvider; label: string }[] = [{ value: "anthropic", label: "Anthropic (Claude)" }];

export default function CreateChatbotPage() {
  const {
    navigate,
    name,
    setName,
    description,
    setDescription,
    status,
    setStatus,
    socratic,
    setSocratic,
    explicaMas,
    setExplicaMas,
    provider,
    handleProviderChange,
    modelOptions,
    selectedModel,
    setModel,
    iconUrl,
    setIconUrl,
    nameError,
    setNameError,
    descriptionError,
    setDescriptionError,
    handleConfirm
  } = useCreateChatbot();

  return (
    <section className="view active">
      <div className={styles.botDetail}>
        <button className={`btn-ghost ${styles.backBtn}`} type="button" onClick={() => navigate("/chatbots")}>
          <ChevronLeftIcon /> Gestión de chatbots
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
          <div className="panel-body">
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
            <div className="form-field field-flush">
              <label htmlFor="new-bot-desc">Descripción breve</label>
              <textarea
                id="new-bot-desc"
                className={`form-input${descriptionError ? " field-error" : ""}`}
                rows={2}
                placeholder="Se mostrará en la card del listado para ayudar a encontrar este chatbot."
                value={description}
                onChange={(e) => {
                  setDescription(e.target.value);
                  if (descriptionError) setDescriptionError(false);
                }}
              />
            </div>
          </div>
        </div>

        <div className={styles.section}>
          <div className="group-head">
            <h2>Comportamiento</h2>
            <p>Esto se configura una vez para todo el equipo — quien use el chatbot no puede cambiarlo desde el propio chat.</p>
          </div>
          <div className="glass">
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
                  <div className="desc">Proveedor y modelo que responde las preguntas de este chatbot.</div>
                </div>
                <div className={styles.configRowControls}>
                  <Select value={provider} onChange={handleProviderChange} options={PROVIDER_OPTIONS} ariaLabel="Proveedor" />
                  {modelOptions.length > 0 ? (
                    <Select value={selectedModel} onChange={setModel} options={modelOptions} ariaLabel="Modelo" />
                  ) : (
                    <span className="hint-sm">
                      Sin modelos registrados — configúralo en Proveedores de IA.
                    </span>
                  )}
                </div>
              </div>
            </div>
          </div>
        </div>

        <div className="form-actions">
          <button className="btn-ghost" type="button" onClick={() => navigate("/chatbots")}>
            <CloseIcon /> Cancelar
          </button>
          <button className="btn-primary" type="button" onClick={handleConfirm}>
            <PlusIcon /> Crear chatbot
          </button>
        </div>
      </div>
    </section>
  );
}
