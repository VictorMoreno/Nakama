import { BrandMarkIcon, HomeIcon, RefreshIcon, WarningTriangleIcon } from "../../components/shared/icons";
import styles from "./ErrorPage.module.scss";

interface ErrorPageProps {
  error?: Error | null;
}

// Standalone screen (same convention as LoginPage — outside AppShell/Router context), since
// this renders from the app-root ErrorBoundary: at that point AppShell's layout, and any
// router/context state above it, may itself be what broke.
export default function ErrorPage({ error }: ErrorPageProps) {
  return (
    <div className={styles.errorScreen}>
      <div className={`${styles.errorCard} glass`}>
        <div className={styles.brand}>
          <div className={styles.brandMark}>
            <BrandMarkIcon />
          </div>
          <div className={styles.brandName}>NAKAMA</div>
        </div>

        <div className={styles.iconWrap}>
          <WarningTriangleIcon />
        </div>

        <h1 className={styles.title}>Algo salió mal</h1>
        <p className={styles.subtitle}>
          Ocurrió un error inesperado y no pudimos continuar con esta pantalla. Probá recargar la
          página — si el problema persiste, avisá al equipo para que lo revise.
        </p>

        {import.meta.env.DEV && error && (
          <details className={styles.details}>
            <summary>Detalles técnicos (solo en desarrollo)</summary>
            <pre>
              {error.message}
              {error.stack ? `\n${error.stack}` : ""}
            </pre>
          </details>
        )}

        <div className={styles.actions}>
          <button className="btn-primary" type="button" onClick={() => window.location.reload()}>
            <RefreshIcon /> Recargar página
          </button>
          <a className="btn-ghost" href="/">
            <HomeIcon /> Volver al inicio
          </a>
        </div>
      </div>
    </div>
  );
}
