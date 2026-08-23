import { useChatbots } from "../../context/ChatbotsContext";
import {
  costByBot,
  enpsByMilestone,
  escalatedToSenior,
  overviewStats,
  resolvedByBot,
  usageByBot,
  weeklyLabels
} from "./analyticsData";
import { TrendChart } from "./TrendChart";
import { UsageChart } from "./UsageChart";

const ENPS_COLORS = ["var(--chart-1)", "var(--chart-2)", "var(--chart-3)"];

export function AnalyticsView() {
  const { chatbots } = useChatbots();

  const globalGaps = Object.values(chatbots)
    .filter((bot) => !bot.archived)
    .flatMap((bot) => bot.gaps.map((g) => ({ ...g, dept: bot.label })))
    .sort((a, b) => b.count - a.count);

  return (
    <section className="view active">
      <div className="stat-grid">
        <div className="stat-card glass">
          <div className="label">Usuarios activos esta semana</div>
          <div className="value">
            {overviewStats.activeUsers}
            <span className="target">{overviewStats.activeUsersNote}</span>
          </div>
          <div className="bar-track">
            <div className="bar-fill" style={{ width: `${overviewStats.activeUsersBar}%` }} />
          </div>
        </div>
        <div className="stat-card glass">
          <div className="label">Preguntas respondidas esta semana</div>
          <div className="value">
            {overviewStats.answeredThisWeek}
            <span className="target">en total</span>
          </div>
          <div className="stat-delta">{overviewStats.answeredDelta}</div>
        </div>
        <div className="stat-card glass">
          <div className="label">Tiempo de seniors liberado</div>
          <div className="value">
            {overviewStats.hoursSaved}
            <span className="target">estimado esta semana</span>
          </div>
          <div className="stat-delta">{overviewStats.hoursSavedDelta}</div>
        </div>
        <div className="stat-card glass">
          <div className="label">Respuestas útiles (global)</div>
          <div className="value">
            {overviewStats.usefulRate}
            <span className="target">% · objetivo 80%</span>
          </div>
          <div className="bar-track">
            <div className="bar-fill" style={{ width: `${overviewStats.usefulRateBar}%` }} />
          </div>
        </div>
      </div>

      <div className="analytics-grid">
        <div className="chart-card glass">
          <div className="panel-head">
            <h2>Preguntas por semana</h2>
            <span className="count">Últimas 8 semanas</span>
          </div>
          <TrendChart weeks={weeklyLabels} resolved={resolvedByBot} escalated={escalatedToSenior} />
        </div>

        <div className="chart-card glass">
          <div className="panel-head">
            <h2>Preguntas resueltas por chatbot</h2>
            <span className="count">Este mes</span>
          </div>
          <UsageChart usage={usageByBot} />
        </div>
      </div>

      <div className="analytics-grid" style={{ marginTop: 14 }}>
        <div className="chart-card glass">
          <div className="panel-head">
            <h2>Coste de LLM por departamento</h2>
            <span className="count">Este mes</span>
          </div>
          <div className="hbar-list">
            {costByBot.map((c) => {
              const pct = Math.min(100, Math.round((c.spentUsd / c.budgetUsd) * 100));
              const over = c.spentUsd > c.budgetUsd;
              return (
                <div className="hbar-row" key={c.key}>
                  <div className="hbar-row-label">
                    <span className="name">{chatbots[c.key]?.label ?? c.key}</span>
                    <span className="value">
                      ${c.spentUsd} / ${c.budgetUsd}
                      {over && (
                        <span className="status-tag danger" style={{ marginLeft: 8 }}>
                          Sobre presupuesto
                        </span>
                      )}
                    </span>
                  </div>
                  <div className="hbar-track">
                    <div
                      className="hbar-fill"
                      style={{ width: `${pct}%`, background: over ? "var(--danger)" : "var(--accent)" }}
                    />
                  </div>
                </div>
              );
            })}
          </div>
        </div>

        <div className="chart-card glass">
          <div className="panel-head">
            <h2>Satisfacción de onboarding (eNPS)</h2>
            <span className="count">Por hito</span>
          </div>
          <div className="hbar-list">
            {enpsByMilestone.map((e, i) => (
              <div className="hbar-row" key={e.milestone}>
                <div className="hbar-row-label">
                  <span className="name">{e.milestone}</span>
                  <span className="value">+{e.score}</span>
                </div>
                <div className="hbar-track">
                  <div className="hbar-fill" style={{ width: `${e.score}%`, background: ENPS_COLORS[i % ENPS_COLORS.length] }} />
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
        <div className="gaps-list">
          {globalGaps.map((g) => (
            <div className="gap-item" key={`${g.dept}-${g.q}`}>
              <div>
                <div className="q">"{g.q}"</div>
                <div className="meta">
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
