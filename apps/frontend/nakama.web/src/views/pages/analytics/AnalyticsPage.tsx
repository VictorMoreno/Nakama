import { useAnalytics } from "./hooks/useAnalytics";
import ActivityChart from "./ActivityChart";
import Select from "../../components/shared/Select";
import { useQuestionActivity, WEEK_RANGE_OPTIONS } from "./hooks/useQuestionActivity";
import { DAY_RANGE_OPTIONS, useUserActivity } from "./hooks/useUserActivity";
import TrendChart from "./TrendChart";
import UsageChart from "./UsageChart";
import styles from "./AnalyticsPage.module.scss";

export default function AnalyticsPage() {
  const { globalGaps, snapshot } = useAnalytics();
  const { activity, days, setDays } = useUserActivity();
  const { activity: questions, weeks, setWeeks } = useQuestionActivity();
  const usageByBot = snapshot.usageByBot.map((u) => ({ key: u.botKey, value: u.value }));
  const activeUsersPct = snapshot.totalUsersCount > 0 ? Math.round((snapshot.activeUsersCount / snapshot.totalUsersCount) * 100) : 0;
  const hasFeedback = snapshot.totalFeedbackCount > 0;
  const usefulRatePct = hasFeedback ? Math.round((snapshot.usefulFeedbackCount / snapshot.totalFeedbackCount) * 100) : 0;

  return (
    <section className="view active">
      <div className={`${styles.statGrid} ${styles.statGridOverview}`}>
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
            <span className={styles.target}>esta semana</span>
          </div>
        </div>
        <div className={`${styles.statCard} glass`}>
          <div className={styles.label}>Respuestas útiles (global)</div>
          {hasFeedback ? (
            <>
              <div className={styles.value}>
                {usefulRatePct}
                <span className={styles.target}>% · objetivo 80%</span>
              </div>
              <div className={styles.barTrack}>
                <div className={styles.barFill} style={{ width: `${usefulRatePct}%` }} />
              </div>
            </>
          ) : (
            <div className={styles.value}>
              <span className={styles.target}>Sin datos todavía</span>
            </div>
          )}
        </div>
      </div>

      <div className={styles.analyticsGrid}>
        <div className={`${styles.chartCard} glass`}>
          <div className="panel-head">
            <h2>Preguntas por semana</h2>
            <Select value={weeks} onChange={setWeeks} options={WEEK_RANGE_OPTIONS} ariaLabel="Rango de semanas de preguntas" />
          </div>
          <TrendChart weeks={questions.weeks} />
        </div>

        <div className={`${styles.chartCard} glass`}>
          <div className="panel-head">
            <h2>Preguntas resueltas por chatbot</h2>
            <span className="count">Este mes</span>
          </div>
          <UsageChart usage={usageByBot} />
        </div>
      </div>

      <div className={`${styles.chartCard} glass`} style={{ marginTop: 14 }}>
        <div className="panel-head">
          <h2>Actividad de usuarios</h2>
          <Select value={days} onChange={setDays} options={DAY_RANGE_OPTIONS} ariaLabel="Rango de días de actividad" />
        </div>
        <div className={styles.statGrid} style={{ gridTemplateColumns: "repeat(3, 1fr)", margin: "10px 0 6px" }}>
          <div className={`${styles.statCard} glass`}>
            <div className={styles.label}>Activos hoy</div>
            <div className={styles.value}>{activity.activeToday}</div>
          </div>
          <div className={`${styles.statCard} glass`}>
            <div className={styles.label}>Activos en 7 días</div>
            <div className={styles.value}>{activity.activeLast7Days}</div>
          </div>
          <div className={`${styles.statCard} glass`}>
            <div className={styles.label}>Activos en 30 días</div>
            <div className={styles.value}>{activity.activeLast30Days}</div>
          </div>
        </div>
        <ActivityChart days={activity.days} />
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
