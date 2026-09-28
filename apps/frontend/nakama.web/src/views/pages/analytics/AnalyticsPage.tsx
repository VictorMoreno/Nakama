import { useAnalytics } from "./hooks/useAnalytics";
import { costByBot, enpsByMilestone, escalatedToSenior, overviewStats, resolvedByBot, weeklyLabels } from "./analyticsData";
import TrendChart from "./TrendChart";
import UsageChart from "./UsageChart";
import styles from "./AnalyticsPage.module.scss";

const ENPS_COLORS = ["var(--chart-1)", "var(--chart-2)", "var(--chart-3)"];

export default function AnalyticsPage() {
  const { chatbots, globalGaps, snapshot } = useAnalytics();
  const usageByBot = snapshot.usageByBot.map((u) => ({ key: u.botKey, value: u.value }));
  const activeUsersPct = snapshot.totalUsersCount > 0 ? Math.round((snapshot.activeUsersCount / snapshot.totalUsersCount) * 100) : 0;

  return (
    <section className="view active">
      <div className={styles.statGrid}>
        <div className={`${styles.statCard} glass`}>
          <div className={styles.label}>Usuarios activos esta semana</div>
          <div className={styles.value}>
            {snapshot.activeUsersCount}
            <span className={styles.target}>de {snapshot.totalUsersCount} con acceso</span>
          </div>
          <div className={styles.barTrack}>
            <div className={styles.barFill} style={{ width: `${activeUsersPct}%` }} />
          </div>
        </div>
        <div className={`${styles.statCard} glass`}>
          <div className={styles.label}>Preguntas respondidas</div>
          <div className={styles.value}>
            {snapshot.answeredThisWeek}
            <span className={styles.target}>en total</span>
          </div>
        </div>
        <div className={`${styles.statCard} glass`}>
          <div className={styles.label}>Tiempo de seniors liberado</div>
          <div className={styles.value}>
            {overviewStats.hoursSaved}
            <span className={styles.target}>estimado esta semana</span>
          </div>
          <div className={styles.statDelta}>{overviewStats.hoursSavedDelta}</div>
        </div>
        <div className={`${styles.statCard} glass`}>
          <div className={styles.label}>Respuestas útiles (global)</div>
          <div className={styles.value}>
            {overviewStats.usefulRate}
            <span className={styles.target}>% · objetivo 80%</span>
          </div>
          <div className={styles.barTrack}>
            <div className={styles.barFill} style={{ width: `${overviewStats.usefulRateBar}%` }} />
          </div>
        </div>
      </div>

      <div className={styles.analyticsGrid}>
        <div className={`${styles.chartCard} glass`}>
          <div className="panel-head">
            <h2>Preguntas por semana</h2>
            <span className="count">Últimas 8 semanas</span>
          </div>
          <TrendChart weeks={weeklyLabels} resolved={resolvedByBot} escalated={escalatedToSenior} />
        </div>

        <div className={`${styles.chartCard} glass`}>
          <div className="panel-head">
            <h2>Preguntas resueltas por chatbot</h2>
            <span className="count">Este mes</span>
          </div>
          <UsageChart usage={usageByBot} />
        </div>
      </div>

      <div className={styles.analyticsGrid} style={{ marginTop: 14 }}>
        <div className={`${styles.chartCard} glass`}>
          <div className="panel-head">
            <h2>Coste de LLM por departamento</h2>
            <span className="count">Este mes</span>
          </div>
          <div className={styles.hbarList}>
            {costByBot.map((c) => {
              const pct = Math.min(100, Math.round((c.spentUsd / c.budgetUsd) * 100));
              const over = c.spentUsd > c.budgetUsd;
              return (
                <div key={c.key}>
                  <div className={styles.hbarRowLabel}>
                    <span className={styles.name}>{chatbots[c.key]?.label ?? c.key}</span>
                    <span className={styles.value}>
                      ${c.spentUsd} / ${c.budgetUsd}
                      {over && (
                        <span className="status-tag danger" style={{ marginLeft: 8 }}>
                          Sobre presupuesto
                        </span>
                      )}
                    </span>
                  </div>
                  <div className={styles.hbarTrack}>
                    <div
                      className={styles.hbarFill}
                      style={{ width: `${pct}%`, background: over ? "var(--danger)" : "var(--accent)" }}
                    />
                  </div>
                </div>
              );
            })}
          </div>
        </div>

        <div className={`${styles.chartCard} glass`}>
          <div className="panel-head">
            <h2>Satisfacción de onboarding (eNPS)</h2>
            <span className="count">Por hito</span>
          </div>
          <div className={styles.hbarList}>
            {enpsByMilestone.map((e, i) => (
              <div key={e.milestone}>
                <div className={styles.hbarRowLabel}>
                  <span className={styles.name}>{e.milestone}</span>
                  <span className={styles.value}>+{e.score}</span>
                </div>
                <div className={styles.hbarTrack}>
                  <div className={styles.hbarFill} style={{ width: `${e.score}%`, background: ENPS_COLORS[i % ENPS_COLORS.length] }} />
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>

      <div className="glass" style={{ marginTop: 14 }}>
        <div className="panel-head">
          <h2>Preguntas sin buena respuesta</h2>
          <span className="count">Todos los chatbots · esta semana</span>
        </div>
        <div className={styles.gapsList}>
          {globalGaps.map((g) => (
            <div className={styles.gapItem} key={`${g.dept}-${g.q}`}>
              <div>
                <div className={styles.q}>&ldquo;{g.q}&rdquo;</div>
                <div className={styles.meta}>
                  {g.dept} · preguntada {g.count} {g.count === 1 ? "vez" : "veces"} esta semana
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
