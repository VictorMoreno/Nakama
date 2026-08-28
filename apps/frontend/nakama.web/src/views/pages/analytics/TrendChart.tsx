import { useRef, useState, type MouseEvent, type TouchEvent } from "react";
import styles from "./TrendChart.module.scss";

interface TrendChartProps {
  weeks: string[];
  resolved: number[];
  escalated: number[];
}

const W = 620;
const H = 200;
const PAD_L = 8;
const PAD_R = 8;
const PAD_T = 10;
const PAD_B = 26;
const PLOT_W = W - PAD_L - PAD_R;
const PLOT_H = H - PAD_T - PAD_B;

export default function TrendChart({ weeks, resolved, escalated }: TrendChartProps) {
  const maxV = 75;
  const svgRef = useRef<SVGSVGElement>(null);
  const [hoverIndex, setHoverIndex] = useState<number | null>(null);

  function xFor(i: number) {
    return PAD_L + (i / (weeks.length - 1)) * PLOT_W;
  }
  function yFor(v: number) {
    return PAD_T + PLOT_H - (v / maxV) * PLOT_H;
  }
  function pathFor(values: number[]) {
    return values.map((v, i) => `${i === 0 ? "M" : "L"}${xFor(i).toFixed(1)},${yFor(v).toFixed(1)}`).join(" ");
  }

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

  const gridFractions = [0.25, 0.5, 0.75];
  const tooltipX = hoverIndex != null ? (xFor(hoverIndex) / W) * 100 : 0;
  const tooltipY = hoverIndex != null ? (yFor(Math.max(resolved[hoverIndex], escalated[hoverIndex])) / H) * 100 : 0;

  return (
    <>
      <div className={styles.chartLegend}>
        <span className={styles.legendItem}>
          <span className={styles.legendDot} style={{ background: "var(--accent)" }} />
          Resueltas por el bot
        </span>
        <span className={styles.legendItem}>
          <span className={styles.legendDot} style={{ background: "var(--cyan)" }} />
          Escaladas a un senior
        </span>
      </div>

      <div className={styles.chartSvgWrap}>
        <svg
          ref={svgRef}
          viewBox={`0 0 ${W} ${H}`}
          role="img"
          aria-label="Preguntas por semana: resueltas por el bot frente a escaladas a un senior"
          onMouseMove={handleMouseMove}
          onMouseLeave={() => setHoverIndex(null)}
          onTouchMove={handleTouchMove}
        >
          {gridFractions.map((f) => {
            const gy = (PAD_T + PLOT_H * f).toFixed(1);
            return <line key={f} x1={PAD_L} y1={gy} x2={W - PAD_R} y2={gy} stroke="var(--border)" strokeWidth="1" />;
          })}

          <path d={pathFor(resolved)} fill="none" stroke="var(--accent)" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
          <path d={pathFor(escalated)} fill="none" stroke="var(--cyan)" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />

          {resolved.map((v, i) => (
            <circle key={`r-${i}`} cx={xFor(i).toFixed(1)} cy={yFor(v).toFixed(1)} r={3} fill="var(--accent)" />
          ))}
          {escalated.map((v, i) => (
            <circle key={`e-${i}`} cx={xFor(i).toFixed(1)} cy={yFor(v).toFixed(1)} r={3} fill="var(--cyan)" />
          ))}

          {hoverIndex != null && (
            <line x1={xFor(hoverIndex)} y1={PAD_T} x2={xFor(hoverIndex)} y2={PAD_T + PLOT_H} stroke="var(--ink-faint)" strokeWidth="1" strokeDasharray="3 3" />
          )}

          {weeks.map((w, i) => (
            <text key={w} x={xFor(i).toFixed(1)} y={H - 6} fontSize="9" fill="var(--ink-faint)" textAnchor="middle">
              {w}
            </text>
          ))}
        </svg>

        {hoverIndex != null && (
          <div className={`${styles.chartTooltip} ${styles.show}`} style={{ left: `${tooltipX}%`, top: `${tooltipY}%` }}>
            <div className={styles.row}>Semana {weeks[hoverIndex]}</div>
            <div className={styles.row}>
              <span className={styles.legendDot} style={{ background: "var(--accent)" }} />
              Resueltas: {resolved[hoverIndex]}
            </div>
            <div className={styles.row}>
              <span className={styles.legendDot} style={{ background: "var(--cyan)" }} />
              Escaladas: {escalated[hoverIndex]}
            </div>
          </div>
        )}
      </div>
    </>
  );
}
