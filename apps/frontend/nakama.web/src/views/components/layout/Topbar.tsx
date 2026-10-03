import { useLocation } from "react-router-dom";
import styles from "./Topbar.module.scss";

function headingFor(pathname: string): { title: string; subtitle: string } {
  if (pathname === "/chat" || pathname.startsWith("/chat/")) {
    return { title: "Chat", subtitle: "" };
  }
  if (pathname === "/formacion" || pathname.startsWith("/formacion/")) {
    return { title: "Formación", subtitle: "Formaciones independientes de los chatbots" };
  }
  if (pathname.startsWith("/analytics")) {
    return { title: "Analítica", subtitle: "Uso de la plataforma — solo administradores" };
  }
  if (pathname.startsWith("/proveedores")) {
    return { title: "Proveedores de IA", subtitle: "Claves de API y catálogo de modelos por proveedor" };
  }
  return { title: "Gestión de chatbots", subtitle: "Crea y administra los chatbots a los que tienes acceso" };
}

export default function Topbar() {
  const { pathname } = useLocation();
  const { title, subtitle } = headingFor(pathname);

  return (
    <div className={styles.topbar}>
      <div className={styles.viewTitle}>
        <h1>{title}</h1>
        {subtitle && <p>{subtitle}</p>}
      </div>
    </div>
  );
}
