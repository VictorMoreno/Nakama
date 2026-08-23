import { useState } from "react";
import { useToast } from "../../../context/ToastContext";
import type { Chatbot } from "../../../types/chatbot";

interface AnalyticsTabProps {
  bot: Chatbot;
}

export function AnalyticsTab({ bot }: AnalyticsTabProps) {
  const { showToast } = useToast();
  const [documented, setDocumented] = useState<Set<number>>(new Set());

  return (
    <div className="tab-panel active">
      <div className="stat-grid">
        {bot.stats.map((s) => (
          <div className="stat-card glass" key={s.label}>
            <div className="label">{s.label}</div>
            <div className="value">
              {s.value}
              <span className="target">{s.unit}</span>
            </div>
            <div className="bar-track">
              <div className="bar-fill" style={{ width: `${s.bar}%` }} />
            </div>
          </div>
        ))}
      </div>

      <div className="glass" style={{ marginTop: 14 }}>
        <div className="panel-head">
          <h2>Preguntas más repetidas</h2>
          <span className="count">Esta semana</span>
        </div>
        <div className="gaps-list">
          {bot.topQuestions.map((q) => (
            <div className="gap-item" key={q.q}>
              <div>
                <div className="q">"{q.q}"</div>
                <div className="meta">
                  Preguntada {q.count} {q.count === 1 ? "vez" : "veces"} esta semana — independientemente de si la
                  respuesta fue buena. Útil para detectar dudas recurrentes del equipo y priorizar contenido.
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      <div className="glass" style={{ marginTop: 14 }}>
        <div className="panel-head">
          <h2>Preguntas sin buena respuesta</h2>
          <span className="count">Esta semana</span>
        </div>
        <div className="gaps-list">
          {bot.gaps.map((g, idx) => {
            const done = documented.has(idx);
            return (
              <div className="gap-item" key={g.q}>
                <div>
                  <div className="q">"{g.q}"</div>
                  <div className="meta">
                    Preguntada {g.count} {g.count === 1 ? "vez" : "veces"} esta semana
                  </div>
                </div>
                <button
                  className={`btn-ghost${done ? " done" : ""}`}
                  type="button"
                  disabled={done}
                  onClick={() => {
                    setDocumented((prev) => new Set(prev).add(idx));
                    showToast("Añadida a la cola de documentación");
                  }}
                >
                  {done ? "Añadido ✓" : "Documentar"}
                </button>
              </div>
            );
          })}
        </div>
      </div>
    </div>
  );
}
