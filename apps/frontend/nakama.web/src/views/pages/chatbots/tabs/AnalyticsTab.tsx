import type { Chatbot } from "../../../../modules/chatbots/domain/Chatbot";
import { useAnalyticsTab } from "./hooks/useAnalyticsTab";
import styles from "./AnalyticsTab.module.scss";

interface AnalyticsTabProps {
  bot: Chatbot;
}

export default function AnalyticsTab({ bot }: AnalyticsTabProps) {
  const { unhelpfulQuestions } = useAnalyticsTab(bot.id);

  return (
    <div className="tab-panel active">
      <div className="glass">
        <div className="panel-head">
          <h2>Preguntas sin buena respuesta</h2>
          <span className="count">Esta semana</span>
        </div>
        {unhelpfulQuestions && (
          <div className={styles.gapsList}>
            {unhelpfulQuestions.length === 0 && (
              <p className={styles.emptyNote}>Ninguna respuesta de este chatbot se ha valorado como no útil esta semana.</p>
            )}
            {unhelpfulQuestions.map((g) => (
              <div className={styles.gapItem} key={g.question}>
                <div>
                  <div className="q">&ldquo;{g.question}&rdquo;</div>
                  <div className="meta">
                    Valorada como no útil {g.count} {g.count === 1 ? "vez" : "veces"} esta semana
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
