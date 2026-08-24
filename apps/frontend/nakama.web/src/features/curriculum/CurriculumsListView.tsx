import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { Select } from "../../components/Select";
import { useChatbots } from "../../context/ChatbotsContext";
import { useCurriculums } from "../../context/CurriculumsContext";
import { useToast } from "../../context/ToastContext";
import { CURRICULUM_STATUS_META, getCurriculumProgressPct, getCurriculumStatus } from "../../lib/status";
import { pluralWord } from "../../lib/text";
import type { CurriculumLifecycleStatus } from "../../types/curriculum";

type StatusFilter = "all" | CurriculumLifecycleStatus;

const STATUS_OPTIONS: { value: StatusFilter; label: string }[] = [
  { value: "all", label: "Todos los estados" },
  ...(Object.keys(CURRICULUM_STATUS_META) as CurriculumLifecycleStatus[]).map((value) => ({
    value,
    label: CURRICULUM_STATUS_META[value].label
  }))
];

export function CurriculumsListView() {
  const { activeCurriculums, archiveCurriculum } = useCurriculums();
  const { chatbots, isAdmin } = useChatbots();
  const { showToast } = useToast();
  const navigate = useNavigate();

  const [deleteKey, setDeleteKey] = useState<string | null>(null);
  const deleteTarget = activeCurriculums.find((c) => c.key === deleteKey);

  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState<StatusFilter>("all");

  const withProgress = activeCurriculums.map((c) => ({ c, pct: getCurriculumProgressPct(c) }));

  const filteredCurriculums = withProgress.filter(({ c }) => {
    const query = search.trim().toLowerCase();
    const matchesSearch =
      query.length === 0 ||
      c.title.toLowerCase().includes(query) ||
      c.audience.toLowerCase().includes(query) ||
      c.description.toLowerCase().includes(query);
    const matchesStatus = statusFilter === "all" || c.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  return (
    <section className="view active">
      <div className="bots-list">
        <div className="bots-toolbar">
          <p className="bots-intro">
            Currículos de formación — independientes de los chatbots, aunque pueden referenciar uno como fuente de
            contexto para generar contenido con IA.
          </p>
          {isAdmin && (
            <button className="btn-primary" type="button" onClick={() => navigate("/formacion/new")}>
              + Crear currículo
            </button>
          )}
        </div>

        <div className="list-filters">
          <input
            className="list-search"
            type="search"
            placeholder="Buscar por título, audiencia o descripción…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            aria-label="Buscar currículos"
          />
          <Select value={statusFilter} onChange={setStatusFilter} options={STATUS_OPTIONS} ariaLabel="Filtrar por estado" />
        </div>

        {filteredCurriculums.length === 0 ? (
          <p className="list-empty">No hay currículos que coincidan con la búsqueda o el filtro.</p>
        ) : (
        <div className="bot-grid">
          {filteredCurriculums.map(({ c, pct }) => {
            const linkedBot = c.linkedChatbotKey ? chatbots[c.linkedChatbotKey] : undefined;
            const status = getCurriculumStatus(c);
            return (
              <div className="bot-card glass" key={c.key}>
                <div className="bot-card-top">
                  <div
                    className="bot-card-icon"
                    style={c.iconUrl ? { backgroundImage: `url(${c.iconUrl})` } : undefined}
                  >
                    <span className={`led ${status.level}`} title={status.label} />
                  </div>
                  <div>
                    <div className="bot-card-name">{c.title}</div>
                    <div className="bot-card-dept">{c.audience}</div>
                  </div>
                </div>
                <p className="bot-card-desc">{c.description}</p>
                <div className="bot-card-meta">
                  <span>{pluralWord(c.modules.length, "módulo", "módulos")}</span>
                  <span>Tu progreso: {pct}%</span>
                </div>
                {linkedBot && (
                  <p className="bot-card-desc" style={{ marginTop: -4 }}>
                    Basado en: {linkedBot.label}
                  </p>
                )}
                <div className="bot-card-actions">
                  <button className="btn-primary" type="button" onClick={() => navigate(`/formacion/${c.key}`)}>
                    Abrir
                  </button>
                  {isAdmin && (
                    <button className="btn-ghost danger" type="button" onClick={() => setDeleteKey(c.key)}>
                      Eliminar
                    </button>
                  )}
                </div>
              </div>
            );
          })}
        </div>
        )}
      </div>

      <ConfirmDialog
        open={deleteTarget != null}
        title={`Eliminar ${deleteTarget?.title ?? ""}`}
        message="¿Seguro que quieres eliminar este currículo? Se perderá su configuración y el progreso registrado. Esta acción no se puede deshacer."
        confirmLabel="Eliminar"
        danger
        onCancel={() => setDeleteKey(null)}
        onConfirm={() => {
          if (deleteKey) {
            archiveCurriculum(deleteKey);
            showToast("Currículo eliminado (demo)");
          }
          setDeleteKey(null);
        }}
      />
    </section>
  );
}
