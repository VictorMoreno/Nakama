import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { CheckIcon, DocumentIcon, ExerciseIcon, LockIcon, QuizIcon, VideoIcon } from "../../components/icons";
import { StatusSelect, type StatusSelectOption } from "../../components/StatusSelect";
import { useChatbots } from "../../context/ChatbotsContext";
import { useCurriculums } from "../../context/CurriculumsContext";
import { useToast } from "../../context/ToastContext";
import { CURRICULUM_STATUS_META, getCurriculumProgressPct } from "../../lib/status";
import type { CurriculumLifecycleStatus, CurriculumModule, ModuleType } from "../../types/curriculum";
import { CreateModuleDialog } from "./CreateModuleDialog";

const TYPE_LABEL: Record<ModuleType, string> = {
  video: "Vídeo",
  documento: "Documento",
  ejercicio: "Ejercicio",
  examen: "Examen"
};

const STATUS_OPTIONS: StatusSelectOption<CurriculumLifecycleStatus>[] = (
  Object.keys(CURRICULUM_STATUS_META) as CurriculumLifecycleStatus[]
).map((value) => ({ value, label: CURRICULUM_STATUS_META[value].label, level: CURRICULUM_STATUS_META[value].level }));

const TYPE_ICON: Record<ModuleType, typeof VideoIcon> = {
  video: VideoIcon,
  documento: DocumentIcon,
  ejercicio: ExerciseIcon,
  examen: QuizIcon
};

interface ModuleRowProps {
  curriculumKey: string;
  module: CurriculumModule;
  index: number;
  locked: boolean;
  done: boolean;
  examScore?: number;
  isAdmin: boolean;
  onDelete: () => void;
}

function ModuleRow({ curriculumKey, module: m, index, locked, done, examScore, isAdmin, onDelete }: ModuleRowProps) {
  const { completeModule, toggleModuleGating } = useCurriculums();
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
      completeModule(curriculumKey, m.id, score);
      showToast(`Examen aprobado con ${score}%`);
    } else {
      showToast(`No has alcanzado la nota mínima (${score}% de ${m.passingScore ?? 70}% requerido) — puedes reintentarlo`);
    }
  }

  return (
    <div className={`module-item${locked ? " locked" : ""}`}>
      <div className="module-item-main">
        <div className="module-item-title">
          {locked ? <LockIcon width={14} height={14} /> : done ? <CheckIcon width={14} height={14} /> : `${index + 1}.`}{" "}
          {m.title}
          <span className={`module-type-badge ${m.type}`}>
            <TypeIcon width={11} height={11} style={{ marginRight: 3, verticalAlign: -1 }} />
            {TYPE_LABEL[m.type]}
          </span>
          {m.aiDraft && <span className="module-draft-badge">Borrador IA</span>}
        </div>
        <div className="module-item-desc">{m.summary}</div>

        {locked ? (
          <div className="module-item-desc">Se desbloquea al completar el módulo anterior.</div>
        ) : (
          <>
            {m.type === "video" && (
              <div className="module-item-desc">
                {m.durationMin} min ·{" "}
                <a href={m.resourceUrl} target="_blank" rel="noreferrer">
                  Ver vídeo
                </a>
              </div>
            )}
            {(m.type === "documento" || m.type === "ejercicio") && (
              <div className="module-item-desc">
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
                  <div className="module-item-desc">
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
              onClick={() => toggleModuleGating(curriculumKey, m.id)}
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
                completeModule(curriculumKey, m.id);
                showToast("Módulo completado");
              }}
            >
              {done ? "Completado ✓" : "Marcar como completado"}
            </button>
          )}
          {!locked && m.type === "examen" && (
            <button className={`btn-ghost${done ? " done" : ""}`} type="button" disabled={done || answers.includes(-1)} onClick={submitExam}>
              {done ? "Aprobado ✓" : "Enviar examen"}
            </button>
          )}

          {isAdmin && (
            <button className="btn-ghost danger" type="button" onClick={onDelete}>
              Eliminar
            </button>
          )}
        </div>
      </div>
    </div>
  );
}

export function CurriculumDetailView() {
  const { key } = useParams<{ key: string }>();
  const { curriculums } = useCurriculums();
  const { chatbots, isAdmin } = useChatbots();
  const navigate = useNavigate();
  const [createOpen, setCreateOpen] = useState(false);
  const [deleteId, setDeleteId] = useState<string | null>(null);

  const c = key ? curriculums[key] : undefined;
  const deleteTarget = c?.modules.find((m) => m.id === deleteId);
  const { removeModule, updateStatus } = useCurriculums();
  const { showToast } = useToast();

  if (!c) {
    return (
      <section className="view active">
        <div className="glass" style={{ padding: 24 }}>
          <p>
            No se ha encontrado este currículo.{" "}
            <button className="config-link" type="button" onClick={() => navigate("/formacion")}>
              Volver a Formación →
            </button>
          </p>
        </div>
      </section>
    );
  }

  const pct = getCurriculumProgressPct(c);
  const linkedBot = c.linkedChatbotKey ? chatbots[c.linkedChatbotKey] : undefined;
  const avgTeamProgress = c.team.length ? Math.round(c.team.reduce((sum, m) => sum + m.progress, 0) / c.team.length) : 0;

  function isLocked(idx: number): boolean {
    if (idx === 0) return false;
    const prev = c!.modules[idx - 1];
    return prev.gated && !c!.myProgress.completedModuleIds.includes(prev.id);
  }

  return (
    <section className="view active">
      <div className="bot-detail">
        <button className="btn-ghost back-btn" type="button" onClick={() => navigate("/formacion")}>
          ← Formación
        </button>

        <div className="bot-detail-head">
          <div>
            <h2>{c.title}</h2>
            <p>
              {c.audience} · {c.description}
              {linkedBot && <> · Basado en la documentación de <strong>{linkedBot.label}</strong></>}
            </p>
          </div>
          {isAdmin ? (
            <StatusSelect
              value={c.status}
              onChange={(status) => updateStatus(c.key, status)}
              options={STATUS_OPTIONS}
              ariaLabel="Estado del currículo"
            />
          ) : (
            <span className={`status-tag ${CURRICULUM_STATUS_META[c.status].level}`}>{CURRICULUM_STATUS_META[c.status].label}</span>
          )}
        </div>

        <div className="glass">
          <div className="curriculum-progress">
            <div className="curriculum-progress-head">
              <span>Tu progreso</span>
              <span className="value">{pct}%</span>
            </div>
            <div className="bar-track">
              <div className="bar-fill" style={{ width: `${pct}%` }} />
            </div>
          </div>

          <div className="panel-head">
            <h2>Módulos del currículo</h2>
            {isAdmin && (
              <button className="btn-primary" type="button" onClick={() => setCreateOpen(true)}>
                + Nuevo módulo
              </button>
            )}
          </div>

          {c.modules.length === 0 ? (
            <p style={{ padding: "0 20px 20px", fontSize: "0.82rem", color: "var(--ink-faint)" }}>
              {isAdmin
                ? 'Este currículo todavía no tiene módulos. Crea el primero con "+ Nuevo módulo".'
                : "Este currículo todavía no tiene módulos."}
            </p>
          ) : (
            <div className="module-list">
              {c.modules.map((m, idx) => (
                <ModuleRow
                  key={m.id}
                  curriculumKey={c.key}
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
                Todavía no hay personas asignadas a este currículo.
              </p>
            ) : (
              <div className="progress-list">
                {c.team.map((m, idx) => (
                  <div className="progress-row" key={`${m.name}-${idx}`}>
                    <span className="name">{m.name}</span>
                    <div className="bar-track">
                      <div className="bar-fill" style={{ width: `${m.progress}%` }} />
                    </div>
                    <span className="pct">{m.progress}%</span>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}
      </div>

      <CreateModuleDialog open={createOpen} curriculumKey={c.key} onClose={() => setCreateOpen(false)} />

      <ConfirmDialog
        open={deleteTarget != null}
        title="Eliminar módulo"
        message={
          <>
            ¿Eliminar <strong>{deleteTarget?.title}</strong> de este currículo?
          </>
        }
        confirmLabel="Eliminar"
        danger
        onCancel={() => setDeleteId(null)}
        onConfirm={() => {
          if (deleteId) {
            removeModule(c.key, deleteId);
            showToast("Módulo eliminado");
          }
          setDeleteId(null);
        }}
      />
    </section>
  );
}
