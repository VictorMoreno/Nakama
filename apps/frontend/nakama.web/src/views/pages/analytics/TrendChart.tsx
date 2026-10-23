import { useRef, useState, type MouseEvent, type TouchEvent } from "react";
import type { WeeklyQuestions } from "../../../modules/analytics/domain/Analytics";
import styles from "./TrendChart.module.scss";

interface TrendChartProps {
  weeks: WeeklyQuestions[];
}

const W = 620;
const H = 200;
const PAD_L = 8;
const PAD_R = 8;
const PAD_T = 10;
const PAD_B = 26;
const PLOT_W = W - PAD_L - PAD_R;
const PLOT_H = H - PAD_T - PAD_B;
const MAX_LABELS = 8;

function shortDate(date: string) {
  const [, month, day] = date.split("-");
  return `${day}/${month}`;
}

export default function TrendChart({ weeks }: TrendChartProps) {
  const svgRef = useRef<SVGSVGElement>(null);
  const [hoverIndex, setHoverIndex] = useState<number | null>(null);

  if (weeks.length < 2) return null;

  const maxV = Math.max(1, ...weeks.map((w) => w.questions));
  const labelStep = Math.ceil(weeks.length / MAX_LABELS);

  function xFor(i: number) {
    return PAD_L + (i / (weeks.length - 1)) * PLOT_W;
  }
  function yFor(v: number) {
    return PAD_T + PLOT_H - (v / maxV) * PLOT_H;
  }
  const path = weeks.map((w, i) => `${i === 0 ? "M" : "L"}${xFor(i).toFixed(1)},${yFor(w.questions).toFixed(1)}`).join(" ");

  function indexFromClientX(clientX: number) {
    const rect = svgRef.current?.getBoundingClientRect();
    if (!rect) return null;
    const relX = ((clientX - rect.left) / rect.width) * W;
    const i = Math.round(((relX - PAD_L) / PLOT_W) * (weeks.length - 1));
    return Math.max(0, Math.min(weeks.length - 1, i));
  }

  function handleMouseMove(e: MouseEvent<SVGSVGElement>) {
    setHoverIndex(indexFromClientX(e.clientX));
  }
  function handleTouchMove(e: TouchEvent<SVGSVGElement>) {
    const touch = e.touches[0];
    if (touch) setHoverIndex(indexFromClientX(touch.clientX));
  }

  const hovered = hoverIndex != null ? weeks[hoverIndex] : null;
  const tooltipX = hoverIndex != null ? (xFor(hoverIndex) / W) * 100 : 0;
  const tooltipY = hovered ? (yFor(hovered.questions) / H) * 100 : 0;

  return (
    <>
      <div className={styles.chartLegend}>
        <span className={styles.legendItem}>
          <span className={styles.legendDot} style={{ background: "var(--accent)" }} />
          Preguntas a los chatbots
        </span>
      </div>

      <div className={styles.chartSvgWrap}>
        <svg
          ref={svgRef}
          viewBox={`0 0 ${W} ${H}`}
          role="img"
          aria-label="Preguntas hechas a los chatbots por semana"
          onMouseMove={handleMouseMove}
          onMouseLeave={() => setHoverIndex(null)}
          onTouchMove={handleTouchMove}
        >
          {[0.25, 0.5, 0.75].map((f) => {
            const gy = (PAD_T + PLOT_H * f).toFixed(1);
            return <line key={f} x1={PAD_L} y1={gy} x2={W - PAD_R} y2={gy} stroke="var(--border)" strokeWidth="1" />;
          })}

          <path d={path} fill="none" stroke="var(--accent)" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />

          {weeks.map((w, i) => (
            <circle key={w.weekStart} cx={xFor(i).toFixed(1)} cy={yFor(w.questions).toFixed(1)} r={3} fill="var(--accent)" />
          ))}

          {hoverIndex != null && (
            <line x1={xFor(hoverIndex)} y1={PAD_T} x2={xFor(hoverIndex)} y2={PAD_T + PLOT_H} stroke="var(--ink-faint)" strokeWidth="1" strokeDasharray="3 3" />
          )}

          {weeks.map((w, i) =>
            i % labelStep === 0 ? (
              <text key={w.weekStart} x={xFor(i).toFixed(1)} y={H - 6} fontSize="9" fill="var(--ink-faint)" textAnchor="middle">
                {shortDate(w.weekStart)}
              </text>
            ) : null
          )}
        </svg>

        {hovered && (
          <div className={`${styles.chartTooltip} ${styles.show}`} style={{ left: `${tooltipX}%`, top: `${tooltipY}%` }}>
            <div className={styles.row}>Semana del {shortDate(hovered.weekStart)}</div>
            <div className={styles.row}>
              <span className={styles.legendDot} style={{ background: "var(--accent)" }} />
              Preguntas: {hovered.questions}
            </div>
          </div>
        )}
      </div>
    </>
  );
}
