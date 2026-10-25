import ConfirmDialog from "../../components/shared/ConfirmDialog";
import {
  ArrowRightIcon,
  ChevronDownIcon,
  ChevronLeftIcon,
  DocumentIcon,
  ExerciseIcon,
  PlusIcon,
  QuizIcon,
  TrashIcon,
  VideoIcon
} from "../../components/shared/icons";
import IconUploadField from "../../components/shared/IconUploadField";
import StatusSelect, { type StatusSelectOption } from "../../components/shared/StatusSelect";
import { useUsers } from "../../shared/contexts/UsersContext";
import { FORMATION_STATUS_META } from "../../../lib/status";
import type { FormationLifecycleStatus, FormationModule, ModuleType } from "../../../modules/formations/domain/Formation";
import AddTeamMemberDialog from "./AddTeamMemberDialog";
import styles from "./FormationManagePage.module.scss";
import FormationDetailsPanel from "./FormationDetailsPanel";
import FormationDocumentsSection from "./FormationDocumentsSection";
import ModuleForm from "./ModuleForm";
import TeamMemberExams from "./TeamMemberExams";
import { FORMATION_TABS, useFormationManage } from "./hooks/useFormationManage";

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

interface ManageModuleRowProps {
  formationKey: string;
  module: FormationModule;
  index: number;
  expanded: boolean;
  onToggleExpand: () => void;
  onDelete: () => void;
}

function ManageModuleRow({ formationKey, module: m, index, expanded, onToggleExpand, onDelete }: ManageModuleRowProps) {
  const TypeIcon = TYPE_ICON[m.type];

  return (
    <div className={`${styles.moduleItem}${expanded ? ` ${styles.expanded}` : ""}`}>
      <div className={styles.moduleItemHeader}>
        <div className={styles.moduleItemMain}>
          <div className={styles.moduleItemTitle}>
            {index + 1}. {m.title}
            <span className={`${styles.moduleTypeBadge} ${styles[m.type]}`}>
              <TypeIcon width={11} height={11} style={{ marginRight: 3, verticalAlign: -1 }} />
              {TYPE_LABEL[m.type]}
            </span>
            {m.aiDraft && <span className={styles.moduleDraftBadge}>Borrador IA</span>}
          </div>

          {!expanded && (
            <>
              <div className={styles.moduleItemDesc}>{m.summary}</div>

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
                <div style={{ display: "flex", flexDirection: "column", gap: 10, marginTop: 4 }}>
                  {m.quiz.map((q) => (
                    <div key={q.question} className={styles.moduleItemDesc}>
                      <strong style={{ color: "var(--ink)" }}>{q.question}</strong>
                      <div style={{ marginTop: 4 }}>
                        {q.options.map((opt, oi) => (
                          <div key={opt} style={oi === q.correctIndex ? { color: "var(--accent)", fontWeight: 600 } : undefined}>
                            {oi === q.correctIndex ? "✓" : "·"} {opt}
                          </div>
                        ))}
                      </div>
                    </div>
                  ))}
                  {m.passingScore !== undefined && <div className={styles.moduleItemDesc}>Nota mínima para aprobar: {m.passingScore}%</div>}
                </div>
              )}
            </>
          )}
        </div>

        <div className={styles.moduleItemActions}>
          <button className="btn-ghost danger" type="button" onClick={onDelete}>
            <TrashIcon /> Eliminar
          </button>
          <button
            className={`btn-ghost ${styles.expandToggle}${expanded ? ` ${styles.expandToggleOpen}` : ""}`}
            type="button"
            onClick={onToggleExpand}
            aria-expanded={expanded}
            aria-label={expanded ? "Colapsar módulo" : "Editar módulo"}
          >
            <ChevronDownIcon />
          </button>
        </div>
      </div>

      {expanded && <ModuleForm formationKey={formationKey} module={m} onClose={onToggleExpand} />}
    </div>
  );
}

export default function FormationManagePage() {
  const {
    c,
    navigate,
    activeTab,
    setActiveTab,
    expandedModuleId,
    toggleModuleExpanded,
    creatingNew,
    startCreating,
    setCreatingNew,
    deleteTarget,
    setDeleteId,
    avgTeamProgress,
    expandedMemberId,
    toggleMemberExpanded,
    teamProgress,
    teamProgressFailed,
    handleUpdateStatus,
    handleUpdateIcon,
    confirmDeleteModule,
    addTeamMemberOpen,
    setAddTeamMemberOpen,
    teamMemberTarget,
    setRemoveTeamMemberUserId,
    confirmRemoveTeamMember
  } = useFormationManage();
  const { findUser } = useUsers();

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
          <StatusSelect value={c.status} onChange={handleUpdateStatus} options={STATUS_OPTIONS} ariaLabel="Estado de la formación" />
        </div>

        <div className="tabs" role="tablist" aria-label="Secciones de la formación">
          {FORMATION_TABS.map((tab) => (
            <button
              key={tab.key}
              className="tab-btn"
              aria-selected={activeTab === tab.key}
              type="button"
              onClick={() => setActiveTab(tab.key)}
            >
              {tab.label}
            </button>
          ))}
        </div>

        {activeTab === "general" && (
          <div className="tab-panel active">
            <FormationDetailsPanel key={c.id} formation={c} />
            <div className="glass" style={{ marginBottom: 14 }}>
              <div className="panel-head">
                <h2>Icono de la formación</h2>
              </div>
              <div style={{ padding: "0 20px 20px" }}>
                <IconUploadField value={c.iconUrl} onChange={handleUpdateIcon} placeholderLabel={c.title} />
              </div>
            </div>
            <div className="glass">
              <div className="panel-head">
                <h2>Módulos de la formación</h2>
              </div>

              <div className={styles.moduleList}>
                {c.modules.map((m, idx) => (
                  <ManageModuleRow
                    key={m.id}
                    formationKey={c.id}
                    module={m}
                    index={idx}
                    expanded={expandedModuleId === m.id}
                    onToggleExpand={() => toggleModuleExpanded(m.id)}
                    onDelete={() => setDeleteId(m.id)}
                  />
                ))}
                {creatingNew ? (
                  <div className={`${styles.moduleItem} ${styles.expanded}`}>
                    <ModuleForm formationKey={c.id} onClose={() => setCreatingNew(false)} />
                  </div>
                ) : (
                  <button className={styles.moduleItemAddTrigger} type="button" onClick={startCreating}>
                    <PlusIcon width={16} height={16} /> Nuevo módulo
                  </button>
                )}
              </div>
            </div>
          </div>
        )}

        {activeTab === "docs" && <FormationDocumentsSection formation={c} />}

        {activeTab === "permisos" && (
          <div className="tab-panel active">
            <div className="glass">
              <div className="panel-head">
                <h2>Progreso del equipo</h2>
                <div style={{ display: "flex", alignItems: "center", gap: 12 }}>
                  <span className="count">Media: {avgTeamProgress}%</span>
                  <button className="btn-primary" type="button" onClick={() => setAddTeamMemberOpen(true)}>
                    <PlusIcon /> Asignar persona
                  </button>
                </div>
              </div>
              {c.team.length === 0 ? (
                <p style={{ padding: "0 20px 20px", fontSize: "0.82rem", color: "var(--ink-faint)" }}>
                  Todavía no hay personas asignadas a esta formación.
                </p>
              ) : (
                <div className={styles.progressList}>
                  {c.team.map((m, idx) => (
                    <div key={`${m.userId}-${idx}`}>
                      <div className={styles.progressRow}>
                        <span className={styles.name}>{findUser(m.userId)?.name ?? "Usuario eliminado"}</span>
                        <div className="bar-track">
                          <div className="bar-fill" style={{ width: `${m.progress}%` }} />
                        </div>
                        <span className={styles.pct}>{m.progress}%</span>
                        <button
                          className="btn-ghost"
                          type="button"
                          aria-expanded={expandedMemberId === m.userId}
                          onClick={() => toggleMemberExpanded(m.userId)}
                        >
                          <QuizIcon /> {expandedMemberId === m.userId ? "Ocultar exámenes" : "Ver exámenes"}
                        </button>
                        <button className="btn-ghost danger" type="button" onClick={() => setRemoveTeamMemberUserId(m.userId)}>
                          <TrashIcon /> Quitar
                        </button>
                      </div>
                      {expandedMemberId === m.userId && (
                        <TeamMemberExams modules={c.modules} progress={teamProgress?.[m.userId]} loading={teamProgress === null && !teamProgressFailed} failed={teamProgressFailed} />
                      )}
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        )}
      </div>

      <AddTeamMemberDialog
        open={addTeamMemberOpen}
        formationKey={c.id}
        existingUserIds={c.team.map((m) => m.userId)}
        onClose={() => setAddTeamMemberOpen(false)}
      />

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

      <ConfirmDialog
        open={teamMemberTarget != null}
        title="Quitar persona"
        message={
          <>
            ¿Quitar a <strong>{teamMemberTarget ? (findUser(teamMemberTarget.userId)?.name ?? "esta persona") : ""}</strong> de esta formación?
          </>
        }
        confirmLabel="Quitar"
        danger
        onCancel={() => setRemoveTeamMemberUserId(null)}
        onConfirm={confirmRemoveTeamMember}
      />
    </section>
  );
}
