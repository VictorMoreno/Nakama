import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { ConfirmDialog } from "../../components/ConfirmDialog";
import { Select } from "../../components/Select";
import { useChatbots } from "../../context/ChatbotsContext";
import { useToast } from "../../context/ToastContext";
import { CHATBOT_STATUS_META, getChatbotStatus } from "../../lib/status";
import { pluralWord } from "../../lib/text";
import type { ChatbotLifecycleStatus } from "../../types/chatbot";

type StatusFilter = "all" | ChatbotLifecycleStatus;

const STATUS_OPTIONS: { value: StatusFilter; label: string }[] = [
  { value: "all", label: "Todos los estados" },
  ...(Object.keys(CHATBOT_STATUS_META) as ChatbotLifecycleStatus[]).map((value) => ({
    value,
    label: CHATBOT_STATUS_META[value].label
  }))
];

export function ChatbotsListView() {
  const { activeChatbots, archiveChatbot } = useChatbots();
  const { showToast } = useToast();
  const navigate = useNavigate();

  const [deleteKey, setDeleteKey] = useState<string | null>(null);
  const deleteTarget = activeChatbots.find((b) => b.key === deleteKey);

  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState<StatusFilter>("all");

  const filteredChatbots = activeChatbots.filter((bot) => {
    const query = search.trim().toLowerCase();
    const matchesSearch =
      query.length === 0 || bot.label.toLowerCase().includes(query) || bot.description.toLowerCase().includes(query);
    const matchesStatus = statusFilter === "all" || bot.status === statusFilter;
    return matchesSearch && matchesStatus;
  });

  return (
    <section className="view active">
      <div className="bots-list">
        <div className="bots-toolbar">
          <p className="bots-intro">
            Chatbots a los que tienes acceso. Cada uno tiene su propia base de documentos, su configuración y su equipo — nada de esto se
            comparte entre chatbots.
          </p>
          <button className="btn-primary" type="button" onClick={() => navigate("/chatbots/new")}>
            + Crear chatbot
          </button>
        </div>

        <div className="list-filters">
          <input
            className="list-search"
            type="search"
            placeholder="Buscar por nombre o descripción…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            aria-label="Buscar chatbots"
          />
          <Select value={statusFilter} onChange={setStatusFilter} options={STATUS_OPTIONS} ariaLabel="Filtrar por estado" />
        </div>

        {filteredChatbots.length === 0 ? (
          <p className="list-empty">No hay chatbots que coincidan con la búsqueda o el filtro.</p>
        ) : (
        <div className="bot-grid">
          {filteredChatbots.map((bot) => {
            const status = getChatbotStatus(bot);
            return (
            <div className="bot-card glass" key={bot.key}>
              <div className="bot-card-top">
                <div className="bot-card-icon">
                  <span className={`led ${status.level}`} title={status.label} />
                </div>
                <div>
                  <div className="bot-card-name">{bot.label}</div>
                  <div className="bot-card-dept">{bot.general ? "Chatbot general de empresa" : "Chatbot de equipo"}</div>
                </div>
              </div>
              <p className="bot-card-desc">{bot.description}</p>
              <div className="bot-card-meta">
                <span>{pluralWord(bot.docs.length, "documento", "documentos")}</span>
                <span>{pluralWord(bot.team.length, "persona", "personas")}</span>
              </div>
              <div className="bot-card-actions">
                <button className="btn-primary" type="button" onClick={() => navigate(`/chatbots/${bot.key}`)}>
                  Abrir
                </button>
                <button className="btn-ghost" type="button" onClick={() => navigate(`/chatbots/${bot.key}?tab=permisos`)}>
                  Permisos
                </button>
                <button className="btn-ghost danger" type="button" onClick={() => setDeleteKey(bot.key)}>
                  Eliminar
                </button>
              </div>
            </div>
            );
          })}
        </div>
        )}
      </div>

      <ConfirmDialog
        open={deleteTarget != null}
        title={`Eliminar ${deleteTarget?.label ?? ""}`}
        message="¿Seguro que quieres eliminar este chatbot? Se perderá su configuración y el acceso para todo el equipo. Esta acción no se puede deshacer."
        confirmLabel="Eliminar"
        danger
        onCancel={() => setDeleteKey(null)}
        onConfirm={() => {
          if (deleteKey) {
            archiveChatbot(deleteKey);
            showToast("Chatbot eliminado (demo)");
          }
          setDeleteKey(null);
        }}
      />
    </section>
  );
}
