import { useEffect, useRef, useState } from "react";
import { useChatbots } from "../../context/ChatbotsContext";
import { useTheme, type ThemeMode } from "../../context/ThemeContext";
import { useToast } from "../../context/ToastContext";
import { currentUser } from "../../data/mockChatbots";
import { ChevronDownIcon, UserIcon } from "../icons";

const THEME_OPTIONS: { mode: ThemeMode; label: string }[] = [
  { mode: "light", label: "Claro" },
  { mode: "dark", label: "Oscuro" }
];

export function UserMenu() {
  const [open, setOpen] = useState(false);
  const rootRef = useRef<HTMLDivElement>(null);
  const { mode, setMode } = useTheme();
  const { isAdmin, setIsAdmin } = useChatbots();
  const { showToast } = useToast();

  useEffect(() => {
    if (!open) return;
    function onPointerDown(e: MouseEvent) {
      if (rootRef.current && !rootRef.current.contains(e.target as Node)) setOpen(false);
    }
    function onKeyDown(e: KeyboardEvent) {
      if (e.key === "Escape") setOpen(false);
    }
    document.addEventListener("mousedown", onPointerDown);
    document.addEventListener("keydown", onKeyDown);
    return () => {
      document.removeEventListener("mousedown", onPointerDown);
      document.removeEventListener("keydown", onKeyDown);
    };
  }, [open]);

  return (
    <div className={`user-menu${open ? " open" : ""}`} ref={rootRef}>
      <button className="user-trigger" type="button" aria-haspopup="true" aria-expanded={open} onClick={() => setOpen((o) => !o)}>
        <span className="user-avatar">
          <UserIcon />
        </span>
        <span>{currentUser.name}</span>
        <ChevronDownIcon className="chevron" />
      </button>

      <div className="user-dropdown">
        <div className="user-dropdown-head">
          <span className="user-avatar lg">
            <UserIcon />
          </span>
          <div>
            <div className="user-dropdown-name">{currentUser.name}</div>
            <div className="user-dropdown-role">{currentUser.roleLine}</div>
          </div>
        </div>

        <div className="user-dropdown-label">Tema</div>
        <div className="theme-options">
          {THEME_OPTIONS.map((opt) => (
            <button
              key={opt.mode}
              className={`theme-opt${mode === opt.mode ? " active" : ""}`}
              type="button"
              onClick={() => setMode(opt.mode)}
            >
              {opt.label}
            </button>
          ))}
        </div>

        <div className="user-dropdown-demo-row">
          <div className="user-dropdown-demo-text">
            <span className="user-dropdown-demo-label">Ver como administrador</span>
            <span className="user-dropdown-demo-hint">Solo demo</span>
          </div>
          <button
            className={`demo-switch${isAdmin ? " active" : ""}`}
            type="button"
            role="switch"
            aria-checked={isAdmin}
            aria-label="Ver como administrador (demo)"
            onClick={() => setIsAdmin(!isAdmin)}
          >
            <span className="demo-switch-thumb" />
          </button>
        </div>

        <button
          className="user-dropdown-logout"
          type="button"
          onClick={() => {
            setOpen(false);
            showToast("Sesión cerrada (demo)");
          }}
        >
          Cerrar sesión
        </button>
      </div>
    </div>
  );
}
