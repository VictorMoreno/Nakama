import { useLocation } from "react-router-dom";
import BotSwitcher from "./BotSwitcher";
import styles from "./Topbar.module.scss";

function headingFor(pathname: string): { title: string; subtitle: string; showSwitcher: boolean } {
  if (pathname === "/chat" || pathname.startsWith("/chat/")) {
    return { title: "Chat", subtitle: "", showSwitcher: true };
  }
  if (pathname === "/formacion" || pathname.startsWith("/formacion/")) {
    return { title: "Formación", subtitle: "Formaciones independientes de los chatbots", showSwitcher: false };
  }
  if (pathname.startsWith("/analytics")) {
    return { title: "Analítica", subtitle: "Uso de la plataforma — solo administradores", showSwitcher: false };
  }
  if (pathname.startsWith("/proveedores")) {
    return { title: "Proveedores de IA", subtitle: "Claves de API y catálogo de modelos por proveedor", showSwitcher: false };
  }
  return { title: "Gestión de chatbots", subtitle: "Crea y administra los chatbots a los que tienes acceso", showSwitcher: false };
}

export default function Topbar() {
  const { pathname } = useLocation();
  const { title, subtitle, showSwitcher } = headingFor(pathname);

  return (
    <div className={styles.topbar}>
      <div className={styles.viewTitle}>
        <h1>{title}</h1>
        {subtitle && <p>{subtitle}</p>}
        {showSwitcher && (
          <div className={styles.viewTitleSwitcher}>
            <BotSwitcher />
          </div>
        )}
      </div>
    </div>
  );
}
