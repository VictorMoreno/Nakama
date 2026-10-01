import ConfirmDialog from "../../components/shared/ConfirmDialog";
import Select from "../../components/shared/Select";
import { ArrowRightIcon, LockIcon, PlusIcon, RecycleBinIcon } from "../../components/shared/icons";
import { getChatbotStatus } from "../../../lib/status";
import { pluralWord } from "../../../lib/text";
import { STATUS_OPTIONS, useChatbotsList } from "./hooks/useChatbotsList";
import styles from "./ChatbotsListPage.module.scss";

export default function ChatbotsListPage() {
  const {
    navigate,
    search,
    setSearch,
    statusFilter,
    setStatusFilter,
    filteredChatbots,
    setDeleteKey,
    deleteTarget,
    handleDeleteConfirm
  } = useChatbotsList();

  return (
    <section className="view active">
      <div className={styles.botsList}>
        <div className={styles.botsToolbar}>
          <p className={styles.botsIntro}>
            Chatbots a los que tienes acceso. Cada uno tiene su propia base de documentos, su configuración y su equipo — nada de esto se
            comparte entre chatbots.
          </p>
          <button className="btn-primary" type="button" onClick={() => navigate("/chatbots/new")}>
            <PlusIcon /> Crear chatbot
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
        <div className={styles.botGrid}>
          {filteredChatbots.map((bot) => {
            const status = getChatbotStatus(bot);
            return (
            <div className={`${styles.botCard} glass`} key={bot.id}>
              <div className={styles.botCardTop}>
                <div
                  className={styles.botCardIcon}
                  style={bot.iconUrl ? { backgroundImage: `url(${bot.iconUrl})` } : undefined}
                >
                  <span className={`led ${status.level}`} title={status.label} />
                </div>
                <div className={styles.botCardTitles}>
                  <div className={styles.botCardName}>{bot.label}</div>
                  <div className={styles.botCardDept}>{bot.general ? "Chatbot general de empresa" : "Chatbot de equipo"}</div>
                </div>
              </div>
              <p className={styles.botCardDesc}>{bot.description}</p>
              <div className={styles.botCardMeta}>
                <span>{pluralWord(bot.docs.length, "documento", "documentos")}</span>
                <span>{pluralWord(bot.team.length, "persona", "personas")}</span>
              </div>
              <div className={styles.botCardActions}>
                <div className={styles.botCardActionsMain}>
                  <button className="btn-primary" type="button" onClick={() => navigate(`/chatbots/${bot.id}`)}>
                    <ArrowRightIcon /> Abrir
                  </button>
                  <button className="btn-ghost" type="button" onClick={() => navigate(`/chatbots/${bot.id}?tab=permisos`)}>
                    <LockIcon /> Permisos
                  </button>
                </div>
                <button className="btn-ghost danger" type="button" onClick={() => setDeleteKey(bot.id)}>
                  <RecycleBinIcon /> Eliminar
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
        onConfirm={handleDeleteConfirm}
      />
    </section>
  );
}
