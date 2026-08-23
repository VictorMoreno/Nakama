import { Select } from "../../../components/Select";
import { useChatbots } from "../../../context/ChatbotsContext";
import type { Chatbot, LlmProvider } from "../../../types/chatbot";

const PROVIDER_OPTIONS: { value: LlmProvider; label: string }[] = [
  { value: "anthropic", label: "Anthropic (Claude)" },
  { value: "openai", label: "OpenAI (GPT)" },
  { value: "local", label: "Modelo local" }
];

interface GeneralTabProps {
  bot: Chatbot;
}

export function GeneralTab({ bot }: GeneralTabProps) {
  const { updateSettings } = useChatbots();

  return (
    <div className="tab-panel active">
      <div className="glass">
        <div className="panel-head">
          <h2>Comportamiento del chatbot</h2>
        </div>
        <p style={{ padding: "0 20px 14px", fontSize: "0.8rem", color: "var(--ink-soft)", maxWidth: "60ch" }}>
          Esto se configura una vez para todo el equipo — quien use el chatbot no puede cambiarlo desde el propio chat.
        </p>
        <div className="config-rows">
          <div className="config-row">
            <div className="config-row-text">
              <div className="title">Modo Socrático</div>
              <div className="desc">El bot guía con preguntas en vez de dar la respuesta directa. Se aplica igual para todo el equipo que use este chatbot.</div>
            </div>
            <button
              className="switch"
              role="switch"
              aria-checked={bot.settings.socratic}
              aria-label="Modo Socrático"
              type="button"
              onClick={() => updateSettings(bot.key, { socratic: !bot.settings.socratic })}
            />
          </div>

          <div className="config-row">
            <div className="config-row-text">
              <div className="title">Explícame más</div>
              <div className="desc">Permite pedir una capa extra de profundidad técnica en cada respuesta.</div>
            </div>
            <button
              className="switch"
              role="switch"
              aria-checked={bot.settings.explicaMas}
              aria-label="Explícame más"
              type="button"
              onClick={() => updateSettings(bot.key, { explicaMas: !bot.settings.explicaMas })}
            />
          </div>

          <div className="config-row">
            <div className="config-row-text">
              <div className="title">Modelo de lenguaje</div>
              <div className="desc">Proveedor que responde las preguntas de este chatbot. Cada chatbot puede usar uno distinto.</div>
            </div>
            <Select
              value={bot.settings.provider}
              onChange={(provider) => updateSettings(bot.key, { provider })}
              options={PROVIDER_OPTIONS}
              ariaLabel="Modelo de lenguaje"
            />
          </div>
        </div>
      </div>
    </div>
  );
}
