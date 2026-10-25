import IconUploadField from "../../../components/shared/IconUploadField";
import Select from "../../../components/shared/Select";
import type { Chatbot, LlmProvider } from "../../../../modules/chatbots/domain/Chatbot";
import { useGeneralTab } from "./hooks/useGeneralTab";
import styles from "./GeneralTab.module.scss";

const PROVIDER_OPTIONS: { value: LlmProvider; label: string }[] = [{ value: "anthropic", label: "Anthropic (Claude)" }];

interface GeneralTabProps {
  bot: Chatbot;
}

export default function GeneralTab({ bot }: GeneralTabProps) {
  const {
    updateSettings,
    updateIcon,
    modelOptions,
    label,
    description,
    labelError,
    descriptionError,
    saving,
    dirty,
    changeLabel,
    changeDescription,
    changeProvider,
    handleSaveDetails
  } = useGeneralTab(bot);

  return (
    <div className="tab-panel active">
      <div className="glass panel-spaced">
        <div className="panel-head">
          <h2>Datos básicos</h2>
        </div>
        <div className="panel-body">
          <div className="form-field">
            <label htmlFor="chatbot-label">Nombre</label>
            <input
              id="chatbot-label"
              type="text"
              className={`form-input${labelError ? " field-error" : ""}`}
              value={label}
              onChange={(e) => changeLabel(e.target.value)}
            />
          </div>
          <div className="form-field">
            <label htmlFor="chatbot-description">Descripción</label>
            <textarea
              id="chatbot-description"
              className={`form-input${descriptionError ? " field-error" : ""}`}
              rows={2}
              value={description}
              onChange={(e) => changeDescription(e.target.value)}
            />
          </div>
          <div className="form-actions">
            <button className="btn-primary" type="button" disabled={!dirty || saving} onClick={handleSaveDetails}>
              Guardar cambios
            </button>
          </div>
        </div>
      </div>

      <div className="glass panel-spaced">
        <div className="panel-head">
          <h2>Icono del chatbot</h2>
        </div>
        <div className="panel-body">
          <IconUploadField
            value={bot.iconUrl}
            onChange={(iconUrl) => updateIcon(bot.id, iconUrl)}
            placeholderLabel={bot.label}
          />
        </div>
      </div>

      <div>
        <div className="group-head">
          <h2>Comportamiento del chatbot</h2>
          <p>Esto se configura una vez para todo el equipo — quien use el chatbot no puede cambiarlo desde el propio chat.</p>
        </div>
        <div className="glass">
          <div className={styles.configRows}>
            <div className={styles.configRow}>
              <div className={styles.configRowText}>
                <div className="title">Modo Socrático</div>
                <div className="desc">El bot guía con preguntas en vez de dar la respuesta directa. Se aplica igual para todo el equipo que use este chatbot.</div>
              </div>
              <button
                className="switch"
                role="switch"
                aria-checked={bot.settings.socratic}
                aria-label="Modo Socrático"
                type="button"
                onClick={() => updateSettings(bot.id, { socratic: !bot.settings.socratic })}
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
                aria-checked={bot.settings.explicaMas}
                aria-label="Explícame más"
                type="button"
                onClick={() => updateSettings(bot.id, { explicaMas: !bot.settings.explicaMas })}
              />
            </div>

            <div className={styles.configRow}>
              <div className={styles.configRowText}>
                <div className="title">Modelo de lenguaje</div>
                <div className="desc">Proveedor y modelo que responde las preguntas de este chatbot. Cada chatbot puede usar uno distinto.</div>
              </div>
              <div className={styles.configRowControls}>
                <Select value={bot.settings.provider} onChange={changeProvider} options={PROVIDER_OPTIONS} ariaLabel="Proveedor" />
                {modelOptions.length > 0 ? (
                  <Select
                    value={bot.settings.model}
                    onChange={(model) => updateSettings(bot.id, { model })}
                    options={modelOptions}
                    ariaLabel="Modelo"
                  />
                ) : (
                  <span className="hint-sm">Sin modelos registrados — configúralo en Proveedores de IA.</span>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
