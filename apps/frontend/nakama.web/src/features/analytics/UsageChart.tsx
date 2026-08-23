import { useChatbots } from "../../context/ChatbotsContext";

interface UsageEntry {
  key: string;
  value: number;
}

interface UsageChartProps {
  usage: UsageEntry[];
}

const CHART_COLORS = ["var(--chart-1)", "var(--chart-2)", "var(--chart-3)"];

export function UsageChart({ usage }: UsageChartProps) {
  const { chatbots } = useChatbots();
  const maxV = Math.max(...usage.map((u) => u.value));

  return (
    <>
      <div className="chart-legend">
        {usage.map((u, i) => (
          <span className="legend-item" key={u.key}>
            <span className="legend-dot" style={{ background: CHART_COLORS[i % CHART_COLORS.length] }} />
            {chatbots[u.key]?.label ?? u.key}
          </span>
        ))}
      </div>

      <div className="hbar-list">
        {usage.map((u, i) => {
          const pct = Math.round((u.value / maxV) * 100);
          const color = CHART_COLORS[i % CHART_COLORS.length];
          return (
            <div className="hbar-row" key={u.key}>
              <div className="hbar-row-label">
                <span className="name">
                  <span className="legend-dot" style={{ background: color }} />
                  {chatbots[u.key]?.label ?? u.key}
                </span>
                <span className="value">{u.value}</span>
              </div>
              <div className="hbar-track">
                <div className="hbar-fill" style={{ width: `${pct}%`, background: color }} />
              </div>
            </div>
          );
        })}
      </div>
    </>
  );
}
