import { useEffect, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import type { IAuthRepository } from "../../../../modules/auth/domain/IAuthRepository";
import { Services } from "../../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../../shared/infrastructure/injection/useInjection";
import { useTheme } from "../../../shared/contexts/ThemeContext";
import { useToast } from "../../../shared/contexts/ToastContext";
import { useUser } from "../../../shared/contexts/UserContext";

export interface DropdownPos {
  bottom: number;
  left: number;
}

export function useUserMenu() {
  const navigate = useNavigate();
  const { currentUser, setCurrentUser, isAdmin } = useUser();
  const authRepo = useInjection<IAuthRepository>(Services.AuthRepository.id);
  const [open, setOpen] = useState(false);
  const [dropdownPos, setDropdownPos] = useState<DropdownPos | null>(null);
  const rootRef = useRef<HTMLDivElement>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const { mode, setMode } = useTheme();
  const { showToast } = useToast();

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

  async function handleLogout() {
    setOpen(false);
    await authRepo.Logout();
    setCurrentUser(null);
    showToast("Sesión cerrada");
    navigate("/login");
  }

  return { currentUser, isAdmin, open, setOpen, dropdownPos, rootRef, dropdownRef, mode, setMode, handleLogout };
}
