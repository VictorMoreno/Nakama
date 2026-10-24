import ConfirmDialog from "../../components/shared/ConfirmDialog";
import Pagination from "../../components/shared/Pagination";
import Select from "../../components/shared/Select";
import { ArrowRightIcon, LockIcon, PlusIcon, RecycleBinIcon, RefreshIcon } from "../../components/shared/icons";
import { CHATBOT_STATUS_META } from "../../../lib/status";
import { pluralWord } from "../../../lib/text";
import { paginationConfig } from "../../../shared/infrastructure/config/PaginationConfig";
import { ARCHIVE_VIEW_OPTIONS } from "../../shared/hooks/usePagedList";
import { STATUS_OPTIONS, useChatbotsList } from "./hooks/useChatbotsList";
import styles from "./ChatbotsListPage.module.scss";

export default function ChatbotsListPage() {
  const {
    navigate,
    isAdmin,
    showingArchived,
    setShowingArchived,
    handleRestore,
    chatbots,
    slide,
    displayedPage,
    isLoading,
    search,
    setSearch,
    statusFilter,
    setStatusFilter,
    pagination,
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
          {isAdmin && (
            <Select
              value={showingArchived ? "archived" : "active"}
              onChange={(view) => setShowingArchived(view === "archived")}
              options={ARCHIVE_VIEW_OPTIONS}
              ariaLabel="Ver chatbots en uso o eliminados"
            />
          )}
        </div>

        {chatbots.length === 0 ? (
          <p className="list-empty">
            {isLoading
              ? "Cargando chatbots…"
              : showingArchived
                ? "No hay chatbots eliminados."
                : "No hay chatbots que coincidan con la búsqueda o el filtro."}
          </p>
        ) : (
        <div
          className={`${styles.botGrid}${slide === "none" ? "" : ` list-slide-${slide}`}`}
          key={displayedPage}
          aria-busy={isLoading}
        >
          {chatbots.map((bot) => {
            const status = CHATBOT_STATUS_META[bot.status];
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
                </div>
              </div>
              <p className={styles.botCardDesc}>{bot.description}</p>
              <div className={styles.botCardMeta}>
                <span>{pluralWord(bot.docsCount, "documento", "documentos")}</span>
                <span>{pluralWord(bot.teamCount, "persona", "personas")}</span>
              </div>
              <div className={styles.botCardActions}>
                {showingArchived ? (
                  <button className="btn-primary" type="button" onClick={() => handleRestore(bot.id)}>
                    <RefreshIcon /> Reactivar
                  </button>
                ) : (
                  <>
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
                  </>
                )}
              </div>
            </div>
            );
          })}
        </div>
        )}

        <Pagination {...pagination} pageSizeOptions={paginationConfig.pageSizeOptions} />
      </div>

      <ConfirmDialog
        open={deleteTarget != null}
        title={`Eliminar ${deleteTarget?.label ?? ""}`}
        message="El chatbot dejará de estar disponible para todo el equipo. Un administrador podrá reactivarlo más adelante."
        confirmLabel="Eliminar"
        danger
        onCancel={() => setDeleteKey(null)}
        onConfirm={handleDeleteConfirm}
      />
    </section>
  );
}
