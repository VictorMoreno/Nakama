import { useLocation } from "react-router-dom";
import { BotSwitcher } from "./BotSwitcher";
import { UserMenu } from "./UserMenu";

function headingFor(pathname: string): { title: string; subtitle: string; showSwitcher: boolean } {
  if (pathname === "/chat" || pathname.startsWith("/chat/")) {
    return { title: "Chat", subtitle: "Vista del desarrollador junior", showSwitcher: true };
  }
  if (pathname === "/formacion" || pathname.startsWith("/formacion/")) {
    return { title: "Formación", subtitle: "Currículos de formación, independientes de los chatbots", showSwitcher: false };
  }
  if (pathname.startsWith("/analytics")) {
    return { title: "Analítica", subtitle: "Uso de la plataforma — solo administradores", showSwitcher: false };
  }
  return { title: "Chatbots", subtitle: "Crea y administra los chatbots a los que tienes acceso", showSwitcher: false };
}

export function Topbar() {
  const { pathname } = useLocation();
  const { title, subtitle, showSwitcher } = headingFor(pathname);

  return (
    <div className="topbar">
      <div className="view-title">
        <h1>{title}</h1>
        <p>{subtitle}</p>
      </div>
      <div style={{ display: "flex", alignItems: "center", gap: 14, flexWrap: "wrap" }}>
        {showSwitcher && <BotSwitcher />}
        <UserMenu />
      </div>
    </div>
  );
}
