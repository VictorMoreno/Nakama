import { type FormEvent, useState } from "react";
import { useNavigate } from "react-router-dom";
import { ArrowRightIcon, BrandMarkIcon, LockIcon, UserIcon } from "../../components/shared/icons";
import styles from "./LoginPage.module.scss";

/**
 * Standalone screen — rendered outside <AppShell> (see Router.tsx), so no Rail/Topbar.
 * The app has no real backend (every repository is a Mock*Repository, see agents.md
 * drift note), so this is a visual/demo login only: any email+password combination
 * "signs in" and lands on /chat, same spirit as the rest of the app's "(demo)" actions
 * (UserMenu's "Cerrar sesión (demo)" toast, the admin-view switch, etc.).
 */
export default function LoginPage() {
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    navigate("/chat");
  }

  return (
    <div className={styles.loginScreen}>
      <div className={`${styles.loginCard} glass`}>
        <div className={styles.brand}>
          <div className={styles.brandMark}>
            <BrandMarkIcon />
          </div>
          <div className={styles.brandName}>NAKAMA</div>
          <p className={styles.brandSub}>Tu tripulación de onboarding</p>
        </div>

        <form className={styles.form} onSubmit={handleSubmit}>
          <div className="form-field">
            <label htmlFor="login-email">Correo</label>
            <div className={styles.inputWithIcon}>
              <UserIcon className={styles.inputIcon} />
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

          <a className={styles.forgotLink} href="#">
            ¿Olvidaste tu contraseña?
          </a>

          <button className="btn-primary" type="submit" style={{ width: "100%" }}>
            <ArrowRightIcon /> Iniciar sesión
          </button>

          <p className={styles.demoHint}>Modo demo — cualquier correo y contraseña te dejan entrar.</p>
        </form>
      </div>
    </div>
  );
}
