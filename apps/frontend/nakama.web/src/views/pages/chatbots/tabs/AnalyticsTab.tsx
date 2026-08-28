import type { Chatbot } from "../../../../modules/chatbots/domain/Chatbot";
import { useAnalyticsTab } from "./hooks/useAnalyticsTab";
import styles from "./AnalyticsTab.module.scss";

interface AnalyticsTabProps {
  bot: Chatbot;
}

export default function AnalyticsTab({ bot }: AnalyticsTabProps) {
  const { documented, markDocumented } = useAnalyticsTab();

  return (
    <div className="tab-panel active">
      <div className={styles.statGrid}>
        {bot.stats.map((s) => (
          <div className={`${styles.statCard} glass`} key={s.label}>
            <div className="label">{s.label}</div>
            <div className="value">
              {s.value}
              <span className="target">{s.unit}</span>
            </div>
            <div className={styles.barTrack}>
              <div className={styles.barFill} style={{ width: `${s.bar}%` }} />
            </div>
          </div>
        ))}
      </div>

      <div className="glass" style={{ marginTop: 14 }}>
        <div className="panel-head">
          <h2>Preguntas más repetidas</h2>
          <span className="count">Esta semana</span>
        </div>
        <div className={styles.gapsList}>
          {bot.topQuestions.map((q) => (
            <div className={styles.gapItem} key={q.q}>
              <div>
                <div className="q">&ldquo;{q.q}&rdquo;</div>
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
        <div className={styles.gapsList}>
          {bot.gaps.map((g, idx) => {
            const done = documented.has(idx);
            return (
              <div className={styles.gapItem} key={g.q}>
                <div>
                  <div className="q">&ldquo;{g.q}&rdquo;</div>
                  <div className="meta">
                    Preguntada {g.count} {g.count === 1 ? "vez" : "veces"} esta semana
                  </div>
                </div>
                <button
                  className={`btn-ghost${done ? " done" : ""}`}
                  type="button"
                  disabled={done}
                  onClick={() => markDocumented(idx)}
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
