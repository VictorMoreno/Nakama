import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Select } from "../../components/Select";
import { StatusSelect, type StatusSelectOption } from "../../components/StatusSelect";
import { useChatbots } from "../../context/ChatbotsContext";
import { useToast } from "../../context/ToastContext";
import { CHATBOT_STATUS_META } from "../../lib/status";
import type { ChatbotLifecycleStatus, LlmProvider } from "../../types/chatbot";

const STATUS_OPTIONS: StatusSelectOption<ChatbotLifecycleStatus>[] = (
  Object.keys(CHATBOT_STATUS_META) as ChatbotLifecycleStatus[]
).map((value) => ({ value, label: CHATBOT_STATUS_META[value].label, level: CHATBOT_STATUS_META[value].level }));

const PROVIDER_OPTIONS: { value: LlmProvider; label: string }[] = [
  { value: "anthropic", label: "Anthropic (Claude)" },
  { value: "openai", label: "OpenAI (GPT)" },
  { value: "local", label: "Modelo local" }
];

export function CreateChatbotView() {
  const navigate = useNavigate();
  const { createChatbot } = useChatbots();
  const { showToast } = useToast();

  const [name, setName] = useState("");
  const [general, setGeneral] = useState(false);
  const [dept, setDept] = useState("");
  const [description, setDescription] = useState("");
  const [status, setStatus] = useState<ChatbotLifecycleStatus>("borrador");
  const [socratic, setSocratic] = useState(false);
  const [explicaMas, setExplicaMas] = useState(true);
  const [provider, setProvider] = useState<LlmProvider>("anthropic");
  const [nameError, setNameError] = useState(false);

  function handleConfirm() {
    const trimmed = name.trim();
    if (!trimmed) {
      setNameError(true);
      return;
    }
    const key = createChatbot({
      name: trimmed,
      dept: dept.trim(),
      general,
      description,
      status,
      settings: { socratic, explicaMas, provider }
    });
    showToast("Chatbot creado");
    navigate(`/chatbots/${key}`);
  }

  return (
    <section className="view active">
      <div className="bot-detail">
        <button className="btn-ghost back-btn" type="button" onClick={() => navigate("/chatbots")}>
          ← Chatbots
        </button>

        <div className="bot-detail-head">
          <div>
            <h2>Crear chatbot</h2>
            <p>Se te añadirá como propietario. Podrás subir documentos e invitar a tu equipo en cuanto lo crees.</p>
          </div>
        </div>

        <div className="glass">
          <div className="panel-head">
            <h2>Datos básicos</h2>
          </div>
          <div style={{ padding: "0 20px 20px" }}>
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

          <div className="config-rows">
            <div className="config-row">
              <div className="config-row-text">
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

            <div className="config-row">
              <div className="config-row-text">
                <div className="title">Estado inicial</div>
                <div className="desc">Determina el color del indicador en el listado. Podrás cambiarlo en cualquier momento.</div>
              </div>
              <StatusSelect value={status} onChange={setStatus} options={STATUS_OPTIONS} ariaLabel="Estado inicial del chatbot" />
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
          <div className="config-rows">
            <div className="config-row">
              <div className="config-row-text">
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

            <div className="config-row">
              <div className="config-row-text">
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

            <div className="config-row">
              <div className="config-row-text">
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
