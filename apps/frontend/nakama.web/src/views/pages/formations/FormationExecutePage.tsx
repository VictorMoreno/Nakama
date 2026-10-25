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
import Dialog from "../../components/shared/Dialog";
import { useToast } from "../../shared/contexts/ToastContext";
import { FORMATION_STATUS_META } from "../../../lib/status";
import type { ExamAttempt, FormationModule, ModuleType } from "../../../modules/formations/domain/Formation";
import CourseCompletedDialog from "./CourseCompletedDialog";
import ExamAttemptsList from "./ExamAttemptsList";
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
  completesFormation: boolean;
  onFormationCompleted: () => void;
}

function ExecuteModuleRow({ formationKey, module: m, index, locked, done, examScore, attempts, completesFormation, onFormationCompleted }: ExecuteModuleRowProps) {
  const { completeModule, submitExam } = useFormations();
  const { showToast } = useToast();
  const TypeIcon = TYPE_ICON[m.type];
  const [answers, setAnswers] = useState<number[]>(() => new Array(m.quiz?.length ?? 0).fill(-1));
  const [busy, setBusy] = useState(false);
  const [examTab, setExamTab] = useState<"examen" | "historial">("examen");
  const [examResult, setExamResult] = useState<{ score: number; passed: boolean } | null>(null);
  const lastAttempt = attempts[attempts.length - 1];

  async function handleComplete() {
    setBusy(true);
    try {
      await completeModule(formationKey, m.id);
      if (completesFormation) onFormationCompleted();
      else showToast("Módulo completado");
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
      if (result.passed && completesFormation) onFormationCompleted();
      else setExamResult({ score: result.score, passed: result.passed });
    } catch {
      showToast("No se pudo entregar el examen — inténtalo de nuevo");
    } finally {
      setBusy(false);
    }
  }

  return (
    <>
      <Dialog
        open={examResult !== null}
        onClose={() => setExamResult(null)}
        title={examResult?.passed ? "Examen aprobado" : "Examen no superado"}
      >
        <p>
          {examResult?.passed
            ? `Has aprobado "${m.title}" con un ${examResult.score}%.`
            : `Has obtenido un ${examResult?.score}% en "${m.title}" y la nota mínima es ${m.passingScore}%. Puedes corregir tus respuestas y reintentarlo.`}
        </p>
        <div className="modal-actions">
          <button className="btn-primary" type="button" onClick={() => setExamResult(null)}>
            <CheckIcon /> Aceptar
          </button>
        </div>
      </Dialog>
      <div className={`${styles.moduleItem}${locked ? ` ${styles.locked}` : ""}`}>
        <div className={styles.moduleItemMain}>
          <div className={styles.moduleItemTitle}>
            {locked ? <LockIcon width={14} height={14} /> : done ? <CheckIcon width={14} height={14} /> : `${index + 1}.`} {m.title}
            <span className={`${styles.moduleTypeBadge} ${styles[m.type]}`}>
              <TypeIcon width={11} height={11} className={styles.typeIcon} />
              {TYPE_LABEL[m.type]}
            </span>
            {m.type === "examen" && m.passingScore !== undefined && (
              <span className={styles.passingScoreBadge}>Nota mínima para aprobar: {m.passingScore}%</span>
            )}
            {m.type === "examen" && (done || lastAttempt) && (
              <span className={`${styles.examStatusBadge} ${done ? styles.examPassed : styles.examFailed}`}>
                {done ? `Aprobado · ${examScore ?? lastAttempt?.score}%` : `No superado · ${lastAttempt?.score}%`}
              </span>
            )}
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
                <div className={styles.quizBody}>
                  <div className="tabs" role="tablist" aria-label="Secciones del examen">
                    <button
                      className="tab-btn"
                      type="button"
                      role="tab"
                      aria-selected={examTab === "examen"}
                      onClick={() => setExamTab("examen")}
                    >
                      Examen
                    </button>
                    <button
                      className="tab-btn"
                      type="button"
                      role="tab"
                      aria-selected={examTab === "historial"}
                      onClick={() => setExamTab("historial")}
                    >
                      Historial de intentos ({attempts.length})
                    </button>
                  </div>
                  {examTab === "historial" &&
                    (attempts.length === 0 ? (
                      <div className={styles.moduleItemDesc}>Todavía no has hecho ningún intento de este examen.</div>
                    ) : (
                      <ExamAttemptsList attempts={attempts} />
                    ))}
                  {examTab === "examen" &&
                    m.quiz.map((q, qi) => (
                      <div key={q.question}>
                        <div className={styles.examQuestion}>{q.question}</div>
                        <div className={styles.examOptions}>
                          {q.options.map((opt, oi) => (
                            <label key={opt} className={styles.examOption}>
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
                </div>
              )}
            </>
          )}
        </div>

        <div className={styles.moduleItemSide}>
          <div className={styles.moduleItemSideRow}>
            {!locked && m.type !== "examen" && (
              <button className={`btn-ghost${done ? " done" : ""}`} type="button" disabled={done || busy} onClick={handleComplete}>
                <CheckIcon /> {done ? "Completado ✓" : "Marcar como completado"}
              </button>
            )}
            {!locked && m.type === "examen" && (
              <button
                className={`btn-ghost${done ? " done" : ""}`}
                type="button"
                disabled={done || busy || answers.includes(-1)}
                onClick={handleSubmitExam}
              >
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
    </>
  );
}

export default function FormationExecutePage() {
  const { c, navigate, pct, isLocked, myProgress } = useFormationExecute();
  const [celebrating, setCelebrating] = useState(false);

  if (!c) {
    return (
      <section className="view active">
        <div className="glass glass-padded">
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
            <p className="panel-body hint">
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
                  completesFormation={c.modules.every((other) => other.id === m.id || myProgress.completedModuleIds.includes(other.id))}
                  onFormationCompleted={() => setCelebrating(true)}
                />
              ))}
            </div>
          )}
        </div>
      </div>
      <CourseCompletedDialog open={celebrating} formationTitle={c.title} onClose={() => setCelebrating(false)} />
    </section>
  );
}
