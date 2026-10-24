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
import { pluralWord } from "../../../lib/text";
import type { ExamAttempt, FormationModule, ModuleType } from "../../../modules/formations/domain/Formation";
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
  attempts: ExamAttempt[];
}

function ExecuteModuleRow({ formationKey, module: m, index, locked, done, examScore, attempts }: ExecuteModuleRowProps) {
  const { completeModule, submitExam } = useFormations();
  const { showToast } = useToast();
  const TypeIcon = TYPE_ICON[m.type];
  const [answers, setAnswers] = useState<number[]>(() => new Array(m.quiz?.length ?? 0).fill(-1));
  const [busy, setBusy] = useState(false);
  const lastAttempt = attempts[attempts.length - 1];

  async function handleComplete() {
    setBusy(true);
    try {
      await completeModule(formationKey, m.id);
      showToast("Módulo completado");
    } catch {
      showToast("No se pudo completar el módulo — inténtalo de nuevo");
    } finally {
      setBusy(false);
    }
  }

  async function handleSubmitExam() {
    setBusy(true);
    try {
      const result = await submitExam(formationKey, m.id, answers);
      showToast(
        result.passed ? `Examen aprobado con ${result.score}%` : `No has superado el examen (${result.score}%) — puedes reintentarlo`
      );
    } catch {
      showToast("No se pudo entregar el examen — inténtalo de nuevo");
    } finally {
      setBusy(false);
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
                {m.passingScore !== undefined && (
                  <div className={styles.moduleItemDesc}>Nota mínima para aprobar: {m.passingScore}%</div>
                )}
                {lastAttempt && (
                  <div className={styles.moduleItemDesc}>
                    {done
                      ? `Aprobado con ${examScore ?? lastAttempt.score}%`
                      : `No superado todavía (último intento: ${lastAttempt.score}%) — puedes corregir tus respuestas y reintentar.`}{" "}
                    · {pluralWord(attempts.length, "intento", "intentos")}
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
              disabled={done || busy}
              onClick={handleComplete}
            >
              <CheckIcon /> {done ? "Completado ✓" : "Marcar como completado"}
            </button>
          )}
          {!locked && m.type === "examen" && (
            <button className={`btn-ghost${done ? " done" : ""}`} type="button" disabled={done || busy || answers.includes(-1)} onClick={handleSubmitExam}>
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
  const { c, navigate, pct, isLocked, myProgress } = useFormationExecute();

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
                  done={myProgress.completedModuleIds.includes(m.id)}
                  examScore={myProgress.examScores[m.id]}
                  attempts={myProgress.examAttempts.filter((attempt) => attempt.moduleId === m.id)}
                />
              ))}
            </div>
          )}
        </div>
      </div>
    </section>
  );
}
