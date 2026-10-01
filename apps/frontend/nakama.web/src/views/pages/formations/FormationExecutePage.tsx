import { useState } from "react";
import {
  ArrowRightIcon,
  CheckIcon,
  ChevronLeftIcon,
  DocumentIcon,
  ExerciseIcon,
  LockIcon,
  QuizIcon,
  SendIcon,
  VideoIcon
} from "../../components/shared/icons";
import { useFormations } from "../../shared/contexts/FormationsContext";
import { useToast } from "../../shared/contexts/ToastContext";
import { FORMATION_STATUS_META } from "../../../lib/status";
import type { FormationModule, ModuleType } from "../../../modules/formations/domain/Formation";
import styles from "./FormationExecutePage.module.scss";
import { useFormationExecute } from "./hooks/useFormationExecute";

const TYPE_LABEL: Record<ModuleType, string> = {
  video: "Vídeo",
  documento: "Documento",
  ejercicio: "Ejercicio",
  examen: "Examen"
};

const TYPE_ICON: Record<ModuleType, typeof VideoIcon> = {
  video: VideoIcon,
  documento: DocumentIcon,
  ejercicio: ExerciseIcon,
  examen: QuizIcon
};

interface ExecuteModuleRowProps {
  formationKey: string;
  module: FormationModule;
  index: number;
  locked: boolean;
  done: boolean;
  examScore?: number;
}

function ExecuteModuleRow({ formationKey, module: m, index, locked, done, examScore }: ExecuteModuleRowProps) {
  const { completeModule } = useFormations();
  const { showToast } = useToast();
  const TypeIcon = TYPE_ICON[m.type];
  const [answers, setAnswers] = useState<number[]>(() => new Array(m.quiz?.length ?? 0).fill(-1));
  const [submitted, setSubmitted] = useState(false);

  function submitExam() {
    if (!m.quiz || m.quiz.length === 0) return;
    const correct = answers.filter((a, i) => a === m.quiz![i].correctIndex).length;
    const score = Math.round((correct / m.quiz.length) * 100);
    const passed = score >= (m.passingScore ?? 70);
    setSubmitted(true);
    if (passed) {
      completeModule(formationKey, m.id, score);
      showToast(`Examen aprobado con ${score}%`);
    } else {
      showToast(`No has alcanzado la nota mínima (${score}% de ${m.passingScore ?? 70}% requerido) — puedes reintentarlo`);
    }
  }

  return (
    <div className={`${styles.moduleItem}${locked ? ` ${styles.locked}` : ""}`}>
      <div className={styles.moduleItemMain}>
        <div className={styles.moduleItemTitle}>
          {locked ? <LockIcon width={14} height={14} /> : done ? <CheckIcon width={14} height={14} /> : `${index + 1}.`}{" "}
          {m.title}
          <span className={`${styles.moduleTypeBadge} ${styles[m.type]}`}>
            <TypeIcon width={11} height={11} style={{ marginRight: 3, verticalAlign: -1 }} />
            {TYPE_LABEL[m.type]}
          </span>
          {m.aiDraft && <span className={styles.moduleDraftBadge}>Borrador IA</span>}
        </div>
        <div className={styles.moduleItemDesc}>{m.summary}</div>

        {locked ? (
          <div className={styles.moduleItemDesc}>Se desbloquea al completar el módulo anterior.</div>
        ) : (
          <>
            {m.type === "video" && (
              <div className={styles.moduleItemDesc}>
                {m.durationMin} min
                <a className={styles.resourceLink} href={m.resourceUrl} target="_blank" rel="noreferrer">
                  <ArrowRightIcon width={12} height={12} /> Ver vídeo
                </a>
              </div>
            )}
            {(m.type === "documento" || m.type === "ejercicio") && (
              <div className={styles.moduleItemDesc}>
                <a className={styles.resourceLink} href={m.resourceUrl} target="_blank" rel="noreferrer">
                  <ArrowRightIcon width={12} height={12} /> {m.type === "documento" ? "Abrir documento" : "Abrir ejercicio"}
                </a>
              </div>
            )}
            {m.type === "examen" && m.quiz && (
              <div style={{ display: "flex", flexDirection: "column", gap: 12, marginTop: 4 }}>
                {m.quiz.map((q, qi) => (
                  <div key={q.question}>
                    <div style={{ fontSize: "0.8rem", color: "var(--ink)", fontWeight: 500, marginBottom: 6 }}>{q.question}</div>
                    <div style={{ display: "flex", flexDirection: "column", gap: 4 }}>
                      {q.options.map((opt, oi) => (
                        <label key={opt} style={{ display: "flex", alignItems: "center", gap: 7, fontSize: "0.78rem", color: "var(--ink-soft)" }}>
                          <input
                            type="radio"
                            name={`${m.id}-q${qi}`}
                            disabled={done}
                            checked={answers[qi] === oi}
                            onChange={() =>
                              setAnswers((prev) => {
                                const next = [...prev];
                                next[qi] = oi;
                                return next;
                              })
                            }
                          />
                          {opt}
                        </label>
                      ))}
                    </div>
                  </div>
                ))}
                {(examScore != null || (submitted && !done)) && (
                  <div className={styles.moduleItemDesc}>
                    {done ? `Aprobado con ${examScore}%` : "No superado todavía — puedes corregir tus respuestas y reintentar."}
                  </div>
                )}
              </div>
            )}
          </>
        )}
      </div>

      <div style={{ display: "flex", flexDirection: "column", alignItems: "flex-end", gap: 10, flex: "none" }}>
        <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
          {!locked && m.type !== "examen" && (
            <button
              className={`btn-ghost${done ? " done" : ""}`}
              type="button"
              disabled={done}
              onClick={() => {
                completeModule(formationKey, m.id);
                showToast("Módulo completado");
              }}
            >
              <CheckIcon /> {done ? "Completado ✓" : "Marcar como completado"}
            </button>
          )}
          {!locked && m.type === "examen" && (
            <button className={`btn-ghost${done ? " done" : ""}`} type="button" disabled={done || answers.includes(-1)} onClick={submitExam}>
              {done ? (
                <>
                  <CheckIcon /> Aprobado ✓
                </>
              ) : (
                <>
                  <SendIcon /> Enviar examen
                </>
              )}
            </button>
          )}
        </div>
      </div>
    </div>
  );
}

export default function FormationExecutePage() {
  const { c, navigate, pct, linkedBot, isLocked } = useFormationExecute();

  if (!c) {
    return (
      <section className="view active">
        <div className="glass" style={{ padding: 24 }}>
          <p>
            No se ha encontrado esta formación.{" "}
            <button className="config-link" type="button" onClick={() => navigate("/formacion")}>
              Volver a Formación →
            </button>
          </p>
        </div>
      </section>
    );
  }

  return (
    <section className="view active">
      <div className={styles.botDetail}>
        <button className={`btn-ghost ${styles.backBtn}`} type="button" onClick={() => navigate("/formacion")}>
          <ChevronLeftIcon /> Formación
        </button>

        <div className={styles.botDetailHead}>
          <div>
            <h2>{c.title}</h2>
            <p>
              {c.audience} · {c.description}
              {linkedBot && <> · Basado en la documentación de <strong>{linkedBot.label}</strong></>}
            </p>
          </div>
          <span className={`status-tag ${FORMATION_STATUS_META[c.status].level}`}>{FORMATION_STATUS_META[c.status].label}</span>
        </div>

        <div className="glass">
          <div className={styles.formationProgress}>
            <div className={styles.formationProgressHead}>
              <span>Tu progreso</span>
              <span className={styles.value}>{pct}%</span>
            </div>
            <div className="bar-track">
              <div className="bar-fill" style={{ width: `${pct}%` }} />
            </div>
          </div>

          <div className="panel-head">
            <h2>Módulos de la formación</h2>
          </div>

          {c.modules.length === 0 ? (
            <p style={{ padding: "0 20px 20px", fontSize: "0.82rem", color: "var(--ink-faint)" }}>
              Esta formación todavía no tiene módulos.
            </p>
          ) : (
            <div className={styles.moduleList}>
              {c.modules.map((m, idx) => (
                <ExecuteModuleRow
                  key={m.id}
                  formationKey={c.id}
                  module={m}
                  index={idx}
                  locked={isLocked(idx)}
                  done={c.myProgress.completedModuleIds.includes(m.id)}
                  examScore={c.myProgress.examScores[m.id]}
                />
              ))}
            </div>
          )}
        </div>
      </div>
    </section>
  );
}
