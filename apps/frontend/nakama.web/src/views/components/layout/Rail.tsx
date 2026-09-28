import { NavLink } from "react-router-dom";
import { useLocalStorageState } from "../../shared/hooks/useLocalStorageState";
import { useUser } from "../../shared/contexts/UserContext";
import { AnalyticsIcon, BrandMarkIcon, ChatIcon, ChevronLeftIcon, GridIcon, LearningIcon } from "../shared/icons";
import styles from "./Rail.module.scss";
import UserMenu from "./UserMenu";

const STORAGE_KEY = "nakama-rail-collapsed";

function isBoolean(value: unknown): value is boolean {
  return typeof value === "boolean";
}

export default function Rail() {
  const { isAdmin } = useUser();
  const [collapsed, setCollapsed] = useLocalStorageState(STORAGE_KEY, false, isBoolean);

  return (
    <aside className={`${styles.rail}${collapsed ? ` ${styles.collapsed}` : ""}`}>
      <button
        className={styles.railToggle}
        type="button"
        aria-expanded={!collapsed}
        aria-label={collapsed ? "Expandir menú" : "Colapsar menú"}
        onClick={() => setCollapsed(!collapsed)}
      >
        <ChevronLeftIcon />
      </button>

      <div className={styles.brand}>
        <div className={styles.brandMark}>
          <BrandMarkIcon />
        </div>
        <div className={styles.brandText}>
          <div className={styles.brandName}>NAKAMA</div>
          <div className={styles.brandSub}>Tu tripulación de onboarding</div>
        </div>
      </div>

      <nav className={styles.nav} aria-label="Secciones">
        <NavLink to="/chat" className={styles.navItem} data-tooltip="Chat">
          <ChatIcon />
          <span>Chat</span>
        </NavLink>
        {isAdmin && (
          <NavLink to="/chatbots" className={styles.navItem} data-tooltip="Gestión de chatbots">
            <GridIcon />
            <span>Gestión de chatbots</span>
          </NavLink>
        )}
        <NavLink to="/formacion" className={styles.navItem} data-tooltip="Formación">
          <LearningIcon />
          <span>Formación</span>
        </NavLink>
        {isAdmin && (
          <NavLink to="/analytics" className={styles.navItem} data-tooltip="Analítica">
            <AnalyticsIcon />
            <span>Analítica</span>
          </NavLink>
        )}
      </nav>

      <div className={styles.railFoot}>
        <UserMenu collapsed={collapsed} />
      </div>
    </aside>
  );
}
