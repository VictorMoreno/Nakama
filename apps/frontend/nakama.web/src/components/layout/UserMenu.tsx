import { useEffect, useRef, useState } from "react";
import { createPortal } from "react-dom";
import { useChatbots } from "../../context/ChatbotsContext";
import { useTheme, type ThemeMode } from "../../context/ThemeContext";
import { useToast } from "../../context/ToastContext";
import { currentUser } from "../../data/mockChatbots";
import { ChevronDownIcon, UserIcon } from "../icons";

const THEME_OPTIONS: { mode: ThemeMode; label: string }[] = [
  { mode: "light", label: "Claro" },
  { mode: "dark", label: "Oscuro" }
];

interface DropdownPos {
  bottom: number;
  left: number;
}

export function UserMenu() {
  const [open, setOpen] = useState(false);
  const [dropdownPos, setDropdownPos] = useState<DropdownPos | null>(null);
  const rootRef = useRef<HTMLDivElement>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const { mode, setMode } = useTheme();
  const { isAdmin, setIsAdmin } = useChatbots();
  const { showToast } = useToast();

  // The dropdown is portaled to <body> (see render below) instead of living inside the
  // rail: nested inside the rail's own blurred/tinted surface, its backdrop-filter would
  // sample that pre-blurred backdrop instead of the page behind it, so the exact same
  // glass CSS as e.g. the status select ends up reading as flatter/less transparent.
  // Portaling it lets it blur the plain page background, matching that reference exactly.
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

  return (
    <div className={`user-menu${open ? " open" : ""}`} ref={rootRef}>
      <button className="user-trigger" type="button" aria-haspopup="true" aria-expanded={open} onClick={() => setOpen((o) => !o)}>
        <span className="user-avatar">
          <UserIcon />
        </span>
        <span>{currentUser.name}</span>
        <ChevronDownIcon className="chevron" />
      </button>

      {open &&
        dropdownPos &&
        createPortal(
          <div
            className="user-dropdown"
            ref={dropdownRef}
            style={{ bottom: dropdownPos.bottom, left: dropdownPos.left }}
          >
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
              <span className="user-dropdown-demo-label">Ver como administrador</span>
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
          </div>,
          document.body
        )}
    </div>
  );
}
