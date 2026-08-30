import { useState } from "react";
import ConfirmDialog from "../../components/shared/ConfirmDialog";
import {
  CheckIcon,
  ChevronLeftIcon,
  DocumentIcon,
  ExerciseIcon,
  LockIcon,
  PlusIcon,
  QuizIcon,
  SendIcon,
  TrashIcon,
  VideoIcon
} from "../../components/shared/icons";
import IconUploadField from "../../components/shared/IconUploadField";
import StatusSelect, { type StatusSelectOption } from "../../components/shared/StatusSelect";
import { useFormations } from "../../shared/contexts/FormationsContext";
import { useToast } from "../../shared/contexts/ToastContext";
import { FORMATION_STATUS_META } from "../../../lib/status";
import type { FormationLifecycleStatus, FormationModule, ModuleType } from "../../../modules/formations/domain/Formation";
import CreateModuleDialog from "./CreateModuleDialog";
import styles from "./FormationDetailPage.module.scss";
import { useFormationDetail } from "./hooks/useFormationDetail";

const TYPE_LABEL: Record<ModuleType, string> = {
  video: "Vídeo",
  documento: "Documento",
  ejercicio: "Ejercicio",
  examen: "Examen"
};

const STATUS_OPTIONS: StatusSelectOption<FormationLifecycleStatus>[] = (
  Object.keys(FORMATION_STATUS_META) as FormationLifecycleStatus[]
).map((value) => ({ value, label: FORMATION_STATUS_META[value].label, level: FORMATION_STATUS_META[value].level }));

const TYPE_ICON: Record<ModuleType, typeof VideoIcon> = {
  video: VideoIcon,
  documento: DocumentIcon,
  ejercicio: ExerciseIcon,
  examen: QuizIcon
};

interface ModuleRowProps {
  formationKey: string;
  module: FormationModule;
  index: number;
  locked: boolean;
  done: boolean;
  examScore?: number;
  isAdmin: boolean;
  onDelete: () => void;
}

function ModuleRow({ formationKey, module: m, index, locked, done, examScore, isAdmin, onDelete }: ModuleRowProps) {
  const { completeModule, toggleModuleGating } = useFormations();
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
                {m.durationMin} min ·{" "}
                <a href={m.resourceUrl} target="_blank" rel="noreferrer">
                  Ver vídeo
                </a>
              </div>
            )}
            {(m.type === "documento" || m.type === "ejercicio") && (
              <div className={styles.moduleItemDesc}>
                <a href={m.resourceUrl} target="_blank" rel="noreferrer">
                  {m.type === "documento" ? "Abrir documento" : "Abrir ejercicio"}
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
        {isAdmin && (
          <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
            <span style={{ fontSize: "0.72rem", color: "var(--ink-faint)", whiteSpace: "nowrap" }}>Bloquea el siguiente</span>
            <button
              className="switch"
              role="switch"
              aria-checked={m.gated}
              aria-label={`Bloquear el siguiente módulo hasta completar "${m.title}"`}
              type="button"
              onClick={() => toggleModuleGating(formationKey, m.id)}
            />
          </div>
        )}

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

          {isAdmin && (
            <button className="btn-ghost danger" type="button" onClick={onDelete}>
              <TrashIcon /> Eliminar
            </button>
          )}
        </div>
      </div>
    </div>
  );
}

export default function FormationDetailPage() {
  const {
    c,
    isAdmin,
    navigate,
    createOpen,
    setCreateOpen,
    deleteTarget,
    setDeleteId,
    pct,
    linkedBot,
    avgTeamProgress,
    isLocked,
    handleUpdateStatus,
    handleUpdateIcon,
    confirmDeleteModule
  } = useFormationDetail();

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
          {isAdmin ? (
            <StatusSelect value={c.status} onChange={handleUpdateStatus} options={STATUS_OPTIONS} ariaLabel="Estado de la formación" />
          ) : (
            <span className={`status-tag ${FORMATION_STATUS_META[c.status].level}`}>{FORMATION_STATUS_META[c.status].label}</span>
          )}
        </div>

        {isAdmin && (
          <div className="glass">
            <div className="panel-head">
              <h2>Icono de la formación</h2>
            </div>
            <div style={{ padding: "0 20px 20px" }}>
              <IconUploadField value={c.iconUrl} onChange={handleUpdateIcon} placeholderLabel={c.title} />
            </div>
          </div>
        )}

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
            {isAdmin && (
              <button className="btn-primary" type="button" onClick={() => setCreateOpen(true)}>
                <PlusIcon /> Nuevo módulo
              </button>
            )}
          </div>

          {c.modules.length === 0 ? (
            <p style={{ padding: "0 20px 20px", fontSize: "0.82rem", color: "var(--ink-faint)" }}>
              {isAdmin
                ? 'Esta formación todavía no tiene módulos. Crea el primero con "+ Nuevo módulo".'
                : "Esta formación todavía no tiene módulos."}
            </p>
          ) : (
            <div className={styles.moduleList}>
              {c.modules.map((m, idx) => (
                <ModuleRow
                  key={m.id}
                  formationKey={c.key}
                  module={m}
                  index={idx}
                  locked={isLocked(idx)}
                  done={c.myProgress.completedModuleIds.includes(m.id)}
                  examScore={c.myProgress.examScores[m.id]}
                  isAdmin={isAdmin}
                  onDelete={() => setDeleteId(m.id)}
                />
              ))}
            </div>
          )}
        </div>

        {isAdmin && (
          <div className="glass" style={{ marginTop: 14 }}>
            <div className="panel-head">
              <h2>Progreso del equipo</h2>
              <span className="count">Media: {avgTeamProgress}%</span>
            </div>
            {c.team.length === 0 ? (
              <p style={{ padding: "0 20px 20px", fontSize: "0.82rem", color: "var(--ink-faint)" }}>
                Todavía no hay personas asignadas a esta formación.
              </p>
            ) : (
              <div className={styles.progressList}>
                {c.team.map((m, idx) => (
                  <div className={styles.progressRow} key={`${m.name}-${idx}`}>
                    <span className={styles.name}>{m.name}</span>
                    <div className="bar-track">
                      <div className="bar-fill" style={{ width: `${m.progress}%` }} />
                    </div>
                    <span className={styles.pct}>{m.progress}%</span>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}
      </div>

      <CreateModuleDialog open={createOpen} formationKey={c.key} onClose={() => setCreateOpen(false)} />

      <ConfirmDialog
        open={deleteTarget != null}
        title="Eliminar módulo"
        message={
          <>
            ¿Eliminar <strong>{deleteTarget?.title}</strong> de esta formación?
          </>
        }
        confirmLabel="Eliminar"
        danger
        onCancel={() => setDeleteId(null)}
        onConfirm={confirmDeleteModule}
      />
    </section>
  );
}
