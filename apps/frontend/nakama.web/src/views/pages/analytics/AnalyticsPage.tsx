import { useAnalytics } from "./hooks/useAnalytics";
import ActivityChart from "./ActivityChart";
import Select from "../../components/shared/Select";
import { useQuestionActivity, WEEK_RANGE_OPTIONS } from "./hooks/useQuestionActivity";
import { DAY_RANGE_OPTIONS, useUserActivity } from "./hooks/useUserActivity";
import TrendChart from "./TrendChart";
import UsageChart from "./UsageChart";
import styles from "./AnalyticsPage.module.scss";

export default function AnalyticsPage() {
  const { unhelpfulQuestions, snapshot } = useAnalytics();
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
                {usefulRatePct}%
                <span className={styles.target}>de las respuestas valoradas</span>
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

      <div className={styles.analyticsGrid}>
        <div className={`${styles.chartCard} glass`}>
          <div className="panel-head">
            <h2>Actividad de usuarios</h2>
            <Select value={days} onChange={setDays} options={DAY_RANGE_OPTIONS} ariaLabel="Rango de días de actividad" />
          </div>
          <ActivityChart days={activity.days} />
        </div>

        <div className={`${styles.chartCard} glass`}>
          <div className="panel-head">
            <h2>Personas activas</h2>
            <span className="count">Distintas por periodo</span>
          </div>
          <div className={styles.kpiList}>
            <div className={styles.kpiRow}>
              <span className={styles.kpiLabel}>Hoy</span>
              <span className={styles.kpiValue}>{activity.activeToday}</span>
            </div>
            <div className={styles.kpiRow}>
              <span className={styles.kpiLabel}>Últimos 7 días</span>
              <span className={styles.kpiValue}>{activity.activeLast7Days}</span>
            </div>
            <div className={styles.kpiRow}>
              <span className={styles.kpiLabel}>Últimos 30 días</span>
              <span className={styles.kpiValue}>{activity.activeLast30Days}</span>
            </div>
          </div>
        </div>
      </div>

      <div className="glass">
        <div className="panel-head">
          <h2>Preguntas sin buena respuesta</h2>
          <span className="count">Todos los chatbots · esta semana</span>
        </div>
        <div className={styles.gapsList}>
          {unhelpfulQuestions.length === 0 && (
            <p className={styles.emptyNote}>Ninguna respuesta se ha valorado como no útil esta semana.</p>
          )}
          {unhelpfulQuestions.map((g) => (
            <div className={styles.gapItem} key={`${g.botKey}-${g.question}`}>
              <div>
                <div className={styles.q}>&ldquo;{g.question}&rdquo;</div>
                <div className={styles.meta}>
                  {g.dept} · valorada como no útil {g.count} {g.count === 1 ? "vez" : "veces"} esta semana
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
