import {
  AlertIcon,
  AnalyticsIcon,
  BrandMarkIcon,
  ChatIcon,
  LearningIcon,
  LockIcon,
  LoginIcon,
  MailIcon
} from "../../components/shared/icons";
import { useLogin } from "./hooks/useLogin";
import styles from "./LoginPage.module.scss";

const FEATURES = [
  { icon: ChatIcon, text: "Chat entrenado con la documentación de tu equipo" },
  { icon: LearningIcon, text: "Rutas de formación guiadas por módulos" },
  { icon: AnalyticsIcon, text: "Analítica de uso y adopción en tiempo real" }
];

export default function LoginPage() {
  const { email, setEmail, password, setPassword, error, submitting, handleSubmit } = useLogin();

  return (
    <div className={styles.loginScreen}>
      <aside className={styles.brandPanel}>
        <div className={styles.brandPanelInner}>
          <div className={styles.brand}>
            <div className={styles.brandMark}>
              <BrandMarkIcon />
            </div>
            <div className={styles.brandName}>NAKAMA</div>
          </div>

          <p className={styles.brandTagline}>Onboarding sin fricción para tu equipo.</p>

          <ul className={styles.featureList}>
            {FEATURES.map(({ icon: Icon, text }) => (
              <li key={text} className={styles.featureItem}>
                <span className={styles.featureIcon}>
                  <Icon />
                </span>
                <span>{text}</span>
              </li>
            ))}
          </ul>
        </div>
      </aside>

      <div className={styles.formPanel}>
        <div className={`${styles.loginCard} glass`}>
          <div className={styles.formHead}>
            <h1 className={styles.formTitle}>Iniciar sesión</h1>
            <p className={styles.formSub}>Accedé con las credenciales de tu cuenta Nakama.</p>
          </div>

          <form className={styles.form} onSubmit={handleSubmit}>
            <div className="form-field">
              <label htmlFor="login-email">Correo</label>
              <div className={styles.inputWithIcon}>
                <MailIcon className={styles.inputIcon} />
                <input
                  id="login-email"
                  type="email"
                  className="form-input"
                  placeholder="tu@empresa.com"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  autoComplete="username"
                  autoFocus
                  required
                />
              </div>
            </div>

            <div className="form-field field-tight">
              <label htmlFor="login-password">Contraseña</label>
              <div className={styles.inputWithIcon}>
                <LockIcon className={styles.inputIcon} />
                <input
                  id="login-password"
                  type="password"
                  className="form-input"
                  placeholder="••••••••"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  autoComplete="current-password"
                  required
                />
              </div>
            </div>

            {error && (
              <div className={styles.errorBanner} role="alert">
                <AlertIcon className={styles.errorIcon} />
                <span>{error}</span>
              </div>
            )}

            <button className="btn-primary btn-block" type="submit" disabled={submitting}>
              <LoginIcon /> Iniciar sesión
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}
