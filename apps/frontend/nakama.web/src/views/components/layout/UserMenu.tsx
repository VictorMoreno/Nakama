import { useEffect, useRef, useState } from "react";
import { createPortal } from "react-dom";
import { useNavigate } from "react-router-dom";
import type { IUserRepository } from "../../../modules/user/domain/IUserRepository";
import type { User } from "../../../modules/user/domain/User";
import { Services } from "../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../shared/infrastructure/injection/useInjection";
import { useChatbots } from "../../shared/contexts/ChatbotsContext";
import { useTheme, type ThemeMode } from "../../shared/contexts/ThemeContext";
import { useToast } from "../../shared/contexts/ToastContext";
import { ChevronDownIcon, UserIcon } from "../shared/icons";
import styles from "./UserMenu.module.scss";

const THEME_OPTIONS: { mode: ThemeMode; label: string }[] = [
  { mode: "light", label: "Claro" },
  { mode: "dark", label: "Oscuro" }
];

interface DropdownPos {
  bottom: number;
  left: number;
}

interface UserMenuProps {
  /** True when the rail is collapsed — shrinks the trigger down to just the avatar. */
  collapsed?: boolean;
}

export default function UserMenu({ collapsed = false }: UserMenuProps) {
  const navigate = useNavigate();
  const userRepo = useInjection<IUserRepository>(Services.UserRepository.id);
  const [currentUser, setCurrentUser] = useState<User | null>(null);
  const [open, setOpen] = useState(false);
  const [dropdownPos, setDropdownPos] = useState<DropdownPos | null>(null);
  const rootRef = useRef<HTMLDivElement>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const { mode, setMode } = useTheme();
  const { isAdmin, setIsAdmin } = useChatbots();
  const { showToast } = useToast();

  useEffect(() => {
    let cancelled = false;
    userRepo.GetCurrent().then((user) => {
      if (!cancelled) setCurrentUser(user);
    });
    return () => {
      cancelled = true;
    };
  }, [userRepo]);

  // The dropdown is portaled to <body> (see render below) instead of living inside the
  // rail so it can escape the rail's own stacking context/overflow and sit above the rest
  // of the page regardless of where the trigger is rendered.
  useEffect(() => {
    if (!open) return;

    function updatePosition() {
      const rect = rootRef.current?.getBoundingClientRect();
      if (!rect) return;
      setDropdownPos({ bottom: window.innerHeight - rect.top + 8, left: rect.left });
    }

    updatePosition();
    window.addEventListener("resize", updatePosition);
    window.addEventListener("scroll", updatePosition, true);

    function onPointerDown(e: MouseEvent) {
      const target = e.target as Node;
      if (rootRef.current?.contains(target) || dropdownRef.current?.contains(target)) return;
      setOpen(false);
    }
    function onKeyDown(e: KeyboardEvent) {
      if (e.key === "Escape") setOpen(false);
    }
    document.addEventListener("mousedown", onPointerDown);
    document.addEventListener("keydown", onKeyDown);
    return () => {
      window.removeEventListener("resize", updatePosition);
      window.removeEventListener("scroll", updatePosition, true);
      document.removeEventListener("mousedown", onPointerDown);
      document.removeEventListener("keydown", onKeyDown);
    };
  }, [open]);

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
                <div className={styles.userDropdownRole}>{currentUser.roleLine}</div>
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

            <div className={styles.userDropdownDemoRow}>
              <span className={styles.userDropdownDemoLabel}>Ver como administrador</span>
              <button
                className={`${styles.demoSwitch}${isAdmin ? ` ${styles.active}` : ""}`}
                type="button"
                role="switch"
                aria-checked={isAdmin}
                aria-label="Ver como administrador (demo)"
                onClick={() => setIsAdmin(!isAdmin)}
              >
                <span className={styles.demoSwitchThumb} />
              </button>
            </div>

            <button
              className={styles.userDropdownLogout}
              type="button"
              onClick={() => {
                setOpen(false);
                showToast("Sesión cerrada (demo)");
                navigate("/login");
              }}
            >
              Cerrar sesión
            </button>
          </div>,
          document.body
        )}
    </div>
  );
}
