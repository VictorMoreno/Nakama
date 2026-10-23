import { useEffect, useState } from "react";
import IconUploadField from "../../../components/shared/IconUploadField";
import Select from "../../../components/shared/Select";
import { useChatbots } from "../../../shared/contexts/ChatbotsContext";
import type { Chatbot, LlmProvider } from "../../../../modules/chatbots/domain/Chatbot";
import type { ILlmProvidersRepository } from "../../../../modules/llmProviders/domain/ILlmProvidersRepository";
import type { LlmProviderConfig } from "../../../../modules/llmProviders/domain/LlmProviderConfig";
import { Services } from "../../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../../shared/infrastructure/injection/useInjection";
import styles from "./GeneralTab.module.scss";

const PROVIDER_OPTIONS: { value: LlmProvider; label: string }[] = [{ value: "anthropic", label: "Anthropic (Claude)" }];

interface GeneralTabProps {
  bot: Chatbot;
}

export default function GeneralTab({ bot }: GeneralTabProps) {
  const { updateSettings, updateIcon } = useChatbots();
  const llmProvidersRepo = useInjection<ILlmProvidersRepository>(Services.LlmProvidersRepository.id);
  const [providers, setProviders] = useState<LlmProviderConfig[]>([]);

  useEffect(() => {
    let cancelled = false;
    llmProvidersRepo.FindAll().then((rows) => {
      if (!cancelled) setProviders(rows);
    });
    return () => {
      cancelled = true;
    };
  }, [llmProvidersRepo]);

  const selectedProvider = providers.find((p) => p.provider === bot.settings.provider);
  const modelOptions = (selectedProvider?.models ?? []).map((m) => ({ value: m.id, label: m.label }));

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
                <Select
                  value={bot.settings.provider}
                  onChange={(provider) => {
                    const firstModel = providers.find((p) => p.provider === provider)?.models[0]?.id ?? "";
                    updateSettings(bot.id, { provider, model: firstModel });
                  }}
                  options={PROVIDER_OPTIONS}
                  ariaLabel="Proveedor"
                />
                {modelOptions.length > 0 ? (
                  <Select
                    value={bot.settings.model}
                    onChange={(model) => updateSettings(bot.id, { model })}
                    options={modelOptions}
                    ariaLabel="Modelo"
                  />
                ) : (
                  <span style={{ fontSize: "0.8rem", color: "var(--ink-soft)" }}>
                    Sin modelos registrados — configúralo en Proveedores de IA.
                  </span>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
