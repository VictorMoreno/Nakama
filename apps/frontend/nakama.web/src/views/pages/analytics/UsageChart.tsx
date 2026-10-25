import { useChatbots } from "../../shared/contexts/ChatbotsContext";
import styles from "./UsageChart.module.scss";

interface UsageEntry {
  key: string;
  value: number;
}

interface UsageChartProps {
  usage: UsageEntry[];
}

const CHART_COLORS = ["var(--chart-1)", "var(--chart-2)", "var(--chart-3)"];

export default function UsageChart({ usage }: UsageChartProps) {
  const { chatbots } = useChatbots();

  if (usage.length === 0) {
    return <p className="hint">Todavía no hay conversaciones registradas.</p>;
  }

  const maxV = Math.max(...usage.map((u) => u.value));

  return (
    <>
      <div className={styles.chartLegend}>
        {usage.map((u, i) => (
          <span className={styles.legendItem} key={u.key}>
            <span className={styles.legendDot} style={{ background: CHART_COLORS[i % CHART_COLORS.length] }} />
            {chatbots[u.key]?.label ?? u.key}
          </span>
        ))}
      </div>

      <div className={styles.hbarList}>
        {usage.map((u, i) => {
          const pct = Math.round((u.value / maxV) * 100);
          const color = CHART_COLORS[i % CHART_COLORS.length];
          return (
            <div key={u.key}>
              <div className={styles.hbarRowLabel}>
                <span className={styles.name}>
                  <span className={styles.legendDot} style={{ background: color }} />
                  {chatbots[u.key]?.label ?? u.key}
                </span>
                <span className={styles.value}>{u.value}</span>
              </div>
              <div className={styles.hbarTrack}>
                <div className={styles.hbarFill} style={{ width: `${pct}%`, background: color }} />
              </div>
            </div>
          );
        })}
      </div>
    </>
  );
}
