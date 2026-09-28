import { type FormEvent, useState } from "react";
import { useNavigate } from "react-router-dom";
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
import { useUser } from "../../shared/contexts/UserContext";
import styles from "./LoginPage.module.scss";

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "";

interface LoginResponse {
  id: string;
  email: string;
  name: string;
  isAdmin: boolean;
}

const FEATURES = [
  { icon: ChatIcon, text: "Chat entrenado con la documentación de tu equipo" },
  { icon: LearningIcon, text: "Rutas de formación guiadas por módulos" },
  { icon: AnalyticsIcon, text: "Analítica de uso y adopción en tiempo real" }
];

export default function LoginPage() {
  const navigate = useNavigate();
  const { setCurrentUser } = useUser();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setSubmitting(true);
    try {
      const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, password })
      });
      if (!response.ok) {
        setError("Correo o contraseña incorrectos.");
        return;
      }
      const user: LoginResponse = await response.json();
      setCurrentUser(user);
      navigate("/chat");
    } finally {
      setSubmitting(false);
    }
  }

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

            <div className="form-field" style={{ marginBottom: 8 }}>
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

            <button className="btn-primary" type="submit" style={{ width: "100%", justifyContent: "center" }} disabled={submitting}>
              <LoginIcon /> Iniciar sesión
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}
