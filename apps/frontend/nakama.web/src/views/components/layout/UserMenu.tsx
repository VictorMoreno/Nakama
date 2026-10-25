import { createPortal } from "react-dom";
import type { ThemeMode } from "../../shared/contexts/ThemeContext";
import { ChevronDownIcon, UserIcon } from "../shared/icons";
import { useUserMenu } from "./hooks/useUserMenu";
import styles from "./UserMenu.module.scss";

const THEME_OPTIONS: { mode: ThemeMode; label: string }[] = [
  { mode: "light", label: "Claro" },
  { mode: "dark", label: "Oscuro" }
];

interface UserMenuProps {
  collapsed?: boolean;
}

export default function UserMenu({ collapsed = false }: UserMenuProps) {
  const { currentUser, isAdmin, open, setOpen, dropdownPos, rootRef, dropdownRef, mode, setMode, handleLogout } = useUserMenu();

  if (!currentUser) return null;

  return (
    <div className={`${styles.userMenu}${open ? ` ${styles.open}` : ""}`} ref={rootRef}>
      <button
        className={`${styles.userTrigger}${collapsed ? ` ${styles.collapsed}` : ""}`}
        type="button"
        aria-haspopup="true"
        aria-expanded={open}
        onClick={() => setOpen((o) => !o)}
      >
        <div className={styles.userTriggerInfo}>
          <span className={styles.userAvatar}>
            <UserIcon />
          </span>
          <span>{currentUser.name}</span>
        </div>
        <ChevronDownIcon className={styles.chevron} />
      </button>

      {open &&
        dropdownPos &&
        createPortal(
          <div
            className={styles.userDropdown}
            ref={dropdownRef}
            style={{ bottom: dropdownPos.bottom, left: dropdownPos.left }}
          >
            <div className={styles.userDropdownHead}>
              <span className={`${styles.userAvatar} ${styles.lg}`}>
                <UserIcon />
              </span>
              <div>
                <div className={styles.userDropdownName}>{currentUser.name}</div>
                <div className={styles.userDropdownRole}>{isAdmin ? "Administrador" : "Miembro"}</div>
              </div>
            </div>

            <div className={styles.userDropdownLabel}>Tema</div>
            <div className={styles.themeOptions}>
              {THEME_OPTIONS.map((opt) => (
                <button
                  key={opt.mode}
                  className={`${styles.themeOpt}${mode === opt.mode ? ` ${styles.active}` : ""}`}
                  type="button"
                  onClick={() => setMode(opt.mode)}
                >
                  {opt.label}
                </button>
              ))}
            </div>

            <button className={styles.userDropdownLogout} type="button" onClick={handleLogout}>
              Cerrar sesión
            </button>
          </div>,
          document.body
        )}
    </div>
  );
}
