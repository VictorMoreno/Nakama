import { useEffect, useState } from "react";
import { NavLink } from "react-router-dom";
import { useChatbots } from "../../context/ChatbotsContext";
import { AnalyticsIcon, BrandMarkIcon, ChatIcon, ChevronLeftIcon, GridIcon, LearningIcon } from "../icons";
import { UserMenu } from "./UserMenu";

const STORAGE_KEY = "nakama-rail-collapsed";

export function Rail() {
  const { isAdmin } = useChatbots();
  const [collapsed, setCollapsed] = useState<boolean>(() => {
    try {
      return localStorage.getItem(STORAGE_KEY) === "1";
    } catch {
      return false;
    }
  });

  useEffect(() => {
    try {
      localStorage.setItem(STORAGE_KEY, collapsed ? "1" : "0");
    } catch {
      // ignore persistence failures
    }
  }, [collapsed]);

  return (
    <aside className={`rail${collapsed ? " collapsed" : ""}`}>
      <button
        className="rail-toggle"
        type="button"
        aria-expanded={!collapsed}
        aria-label={collapsed ? "Expandir menú" : "Colapsar menú"}
        onClick={() => setCollapsed((c) => !c)}
      >
        <ChevronLeftIcon />
      </button>

      <div className="brand">
        <div className="brand-mark">
          <BrandMarkIcon />
        </div>
        <div className="brand-text">
          <div className="brand-name">NAKAMA</div>
          <div className="brand-sub">Tu tripulación de onboarding</div>
        </div>
      </div>

      <nav className="nav" aria-label="Secciones">
        <NavLink to="/chat" className="nav-item" data-tooltip="Chat">
          <ChatIcon />
          <span>Chat</span>
        </NavLink>
        {isAdmin && (
          <NavLink to="/chatbots" className="nav-item" data-tooltip="Chatbots">
            <GridIcon />
            <span>Chatbots</span>
          </NavLink>
        )}
        <NavLink to="/formacion" className="nav-item" data-tooltip="Formación">
          <LearningIcon />
          <span>Formación</span>
        </NavLink>
        {isAdmin && (
          <NavLink to="/analytics" className="nav-item" data-tooltip="Analítica">
            <AnalyticsIcon />
            <span>Analítica</span>
          </NavLink>
        )}
      </nav>

      <div className="rail-foot">
        <UserMenu />
      </div>
    </aside>
  );
}
