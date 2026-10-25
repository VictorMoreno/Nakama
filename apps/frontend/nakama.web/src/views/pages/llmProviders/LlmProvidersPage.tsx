import { useState } from "react";
import { CloseIcon, PlusIcon } from "../../components/shared/icons";
import type { LlmModelOption, LlmProviderConfig } from "../../../modules/llmProviders/domain/LlmProviderConfig";
import { KNOWN_PROVIDERS, useLlmProviders } from "./hooks/useLlmProviders";
import styles from "./LlmProvidersPage.module.scss";

interface ProviderPanelProps {
  config: LlmProviderConfig;
  onSave: (apiKey: string, enabled: boolean, models: LlmModelOption[]) => void;
}

function ProviderPanel({ config, onSave }: ProviderPanelProps) {
  const [apiKey, setApiKey] = useState("");
  const [enabled, setEnabled] = useState(config.enabled);
  const [models, setModels] = useState<LlmModelOption[]>(config.models);

  function updateModel(index: number, patch: Partial<LlmModelOption>) {
    setModels((prev) => prev.map((m, i) => (i === index ? { ...m, ...patch } : m)));
  }

  function removeModel(index: number) {
    setModels((prev) => prev.filter((_, i) => i !== index));
  }

  function addModel() {
    setModels((prev) => [...prev, { id: "", label: "" }]);
  }

  function handleSave() {
    onSave(apiKey, enabled, models.filter((m) => m.id.trim() !== ""));
  }

  return (
    <>
      <div className="glass">
        <div className="panel-head">
          <h2>Clave y catálogo de modelos</h2>
          <span className={`status-tag ${config.configured ? "ok" : "muted"}`}>{config.configured ? "Configurado" : "Sin configurar"}</span>
        </div>

        <div className="panel-body">
          <div className="form-field">
            <label htmlFor={`${config.provider}-api-key`}>Clave de API</label>
            <input
              id={`${config.provider}-api-key`}
              type="password"
              className="form-input"
              placeholder={config.configured ? "•••••••••••• (ya configurada, dejar vacío para no cambiarla)" : "Pegar clave de API"}
              value={apiKey}
              onChange={(e) => setApiKey(e.target.value)}
            />
            <p className={styles.fieldHint}>
              La clave nunca se muestra una vez guardada — solo se puede reemplazar.
            </p>
          </div>

          <div className={`form-field ${styles.enabledRow}`}>
            <button className="switch" role="switch" aria-checked={enabled} aria-label="Habilitado" type="button" onClick={() => setEnabled((v) => !v)} />
            <label className={styles.enabledLabel}>Habilitado</label>
          </div>

          <div className="form-field field-flush">
            <label>Modelos disponibles</label>
            {models.length === 0 && (
              <p className={styles.emptyHint}>
                Todavía no hay modelos registrados para este proveedor.
              </p>
            )}
            {models.map((model, index) => (
              <div key={index} className={styles.modelRow}>
                <input
                  className={`form-input ${styles.modelInput}`}
                  placeholder="id del modelo (ej. claude-opus-5-5)"
                  value={model.id}
                  onChange={(e) => updateModel(index, { id: e.target.value })}
                />
                <input
                  className={`form-input ${styles.modelInput}`}
                  placeholder="nombre visible (ej. Claude Opus 5.5)"
                  value={model.label}
                  onChange={(e) => updateModel(index, { label: e.target.value })}
                />
                <button className={`btn-ghost ${styles.modelRemove}`} type="button" aria-label="Quitar modelo" onClick={() => removeModel(index)}>
                  <CloseIcon />
                </button>
              </div>
            ))}
            <button className="btn-ghost" type="button" onClick={addModel}>
              <PlusIcon /> Añadir modelo
            </button>
          </div>
        </div>
      </div>

      <div className={styles.pageActions}>
        <button className="btn-primary" type="button" onClick={handleSave}>
          Guardar
        </button>
      </div>
    </>
  );
}

export default function LlmProvidersPage() {
  const { configFor, register, loading, revision, activeTab, setActiveTab } = useLlmProviders();

  if (loading) return null;

  return (
    <section className="view active">
      <div className="tabs" role="tablist" aria-label="Proveedores de IA">
        {KNOWN_PROVIDERS.map((p) => (
          <button key={p.value} className="tab-btn" aria-selected={activeTab === p.value} type="button" onClick={() => setActiveTab(p.value)}>
            {p.label}
          </button>
        ))}
      </div>

      {KNOWN_PROVIDERS.map((p) => (
        <div key={p.value} className={`tab-panel${activeTab === p.value ? " active" : ""}`}>
          <ProviderPanel key={revision} config={configFor(p.value)} onSave={(apiKey, enabled, models) => void register(p.value, apiKey, enabled, models)} />
        </div>
      ))}
    </section>
  );
}
