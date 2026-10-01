import { useNavigate } from "react-router-dom";
import ConfirmDialog from "../../components/shared/ConfirmDialog";
import Select from "../../components/shared/Select";
import { ArrowRightIcon, LockIcon, PlusIcon, RecycleBinIcon } from "../../components/shared/icons";
import { FORMATION_STATUS_META, getFormationStatus } from "../../../lib/status";
import { useFormationsList, type StatusFilter } from "./hooks/useFormationsList";
import type { FormationLifecycleStatus } from "../../../modules/formations/domain/Formation";
import styles from "./FormationsListPage.module.scss";

const STATUS_OPTIONS: { value: StatusFilter; label: string }[] = [
  { value: "all", label: "Todos los estados" },
  ...(Object.keys(FORMATION_STATUS_META) as FormationLifecycleStatus[]).map((value) => ({
    value,
    label: FORMATION_STATUS_META[value].label
  }))
];

export default function FormationsListPage() {
  const navigate = useNavigate();
  const {
    isAdmin,
    search,
    setSearch,
    statusFilter,
    setStatusFilter,
    filteredFormations,
    deleteTarget,
    setDeleteKey,
    confirmDelete
  } = useFormationsList();

  return (
    <section className="view active">
      <div className={styles.botsList}>
        <div className={styles.botsToolbar}>
          <p className={styles.botsIntro}>
            Formaciones independientes de los chatbots, aunque pueden referenciar uno como fuente de contexto para
            generar contenido con IA.
          </p>
          {isAdmin && (
            <button className="btn-primary" type="button" onClick={() => navigate("/formacion/new")}>
              <PlusIcon /> Crear formación
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
            aria-label="Buscar formaciones"
          />
          <Select value={statusFilter} onChange={setStatusFilter} options={STATUS_OPTIONS} ariaLabel="Filtrar por estado" />
        </div>

        {filteredFormations.length === 0 ? (
          <p className="list-empty">No hay formaciones que coincidan con la búsqueda o el filtro.</p>
        ) : (
        <div className={styles.botGrid}>
          {filteredFormations.map(({ c, pct }) => {
            const status = getFormationStatus(c);
            return (
              <div className={`${styles.botCard} glass`} key={c.id}>
                <div className={styles.botCardTop}>
                  <div
                    className={styles.botCardIcon}
                    style={c.iconUrl ? { backgroundImage: `url(${c.iconUrl})` } : undefined}
                  >
                    <span className={`led ${status.level}`} title={status.label} />
                  </div>
                  <div className={styles.botCardTitles}>
                    <div className={styles.botCardName}>{c.title}</div>
                    <div className={styles.botCardDept}>{c.audience}</div>
                  </div>
                </div>
                <p className={styles.botCardDesc}>{c.description}</p>
                <div className={styles.progressRow}>
                  <div className="bar-track">
                    <div className="bar-fill" style={{ width: `${pct}%` }} />
                  </div>
                  <span className={styles.progressPct}>{pct}%</span>
                </div>
                <div className={styles.botCardActions}>
                  <div className={styles.botCardActionsMain}>
                    {c.isEnrolled && (
                      <button className="btn-primary" type="button" onClick={() => navigate(`/formacion/${c.id}`)}>
                        <ArrowRightIcon /> Abrir
                      </button>
                    )}
                    {isAdmin && (
                      <button className="btn-ghost" type="button" onClick={() => navigate(`/formacion/${c.id}/gestionar`)}>
                        <LockIcon /> Gestionar
                      </button>
                    )}
                  </div>
                  {isAdmin && (
                    <button className="btn-ghost danger" type="button" onClick={() => setDeleteKey(c.id)}>
                      <RecycleBinIcon /> Eliminar
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
        message="¿Seguro que quieres eliminar esta formación? Se perderá su configuración y el progreso registrado. Esta acción no se puede deshacer."
        confirmLabel="Eliminar"
        danger
        onCancel={() => setDeleteKey(null)}
        onConfirm={confirmDelete}
      />
    </section>
  );
}
