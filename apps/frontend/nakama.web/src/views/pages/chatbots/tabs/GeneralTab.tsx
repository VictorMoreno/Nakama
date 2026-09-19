import IconUploadField from "../../../components/shared/IconUploadField";
import Select from "../../../components/shared/Select";
import { useChatbots } from "../../../shared/contexts/ChatbotsContext";
import type { Chatbot, LlmProvider } from "../../../../modules/chatbots/domain/Chatbot";
import styles from "./GeneralTab.module.scss";

const PROVIDER_OPTIONS: { value: LlmProvider; label: string }[] = [
  { value: "anthropic", label: "Anthropic (Claude)" },
  { value: "openai", label: "OpenAI (GPT)" },
  { value: "local", label: "Modelo local" }
];

interface GeneralTabProps {
  bot: Chatbot;
}

export default function GeneralTab({ bot }: GeneralTabProps) {
  const { updateSettings, updateIcon } = useChatbots();

  return (
    <div className="tab-panel active">
      <div className="glass" style={{ marginBottom: 14 }}>
        <div className="panel-head">
          <h2>Icono del chatbot</h2>
        </div>
        <div style={{ padding: "0 20px 20px" }}>
          <IconUploadField
            value={bot.iconUrl}
            onChange={(iconUrl) => updateIcon(bot.id, iconUrl)}
            placeholderLabel={bot.label}
          />
        </div>
      </div>

      <div className="glass">
        <div className="panel-head">
          <h2>Comportamiento del chatbot</h2>
        </div>
        <p style={{ padding: "0 20px 14px", fontSize: "0.8rem", color: "var(--ink-soft)", maxWidth: "60ch" }}>
          Esto se configura una vez para todo el equipo — quien use el chatbot no puede cambiarlo desde el propio chat.
        </p>
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
              <div className="desc">Proveedor que responde las preguntas de este chatbot. Cada chatbot puede usar uno distinto.</div>
            </div>
            <Select
              value={bot.settings.provider}
              onChange={(provider) => updateSettings(bot.id, { provider })}
              options={PROVIDER_OPTIONS}
              ariaLabel="Modelo de lenguaje"
            />
          </div>
        </div>
      </div>
    </div>
  );
}
