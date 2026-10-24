import { pluralWord } from "../../../lib/text";
import { ArrowRightIcon, ChatIcon, LearningIcon, QuizIcon } from "../../components/shared/icons";
import { useHome } from "./hooks/useHome";
import styles from "./HomePage.module.scss";

export default function HomePage() {
  const {
    firstName,
    summary,
    failed,
    continueFormation,
    otherFormations,
    pendingExams,
    completedFormations,
    totalFormations,
    latestConversation,
    latestConversationBot,
    navigate
  } = useHome();

  if (failed) {
    return (
      <section className="view active">
        <p className="list-empty">No se pudo cargar tu resumen — recarga la página para reintentarlo.</p>
      </section>
    );
  }

  const hasPending = pendingExams.length > 0 || otherFormations.length > 0;

  return (
    <section className="view active">
      <div className={styles.home}>
        <header className={styles.greeting}>
          <h2>Hola, {firstName}</h2>
          <p>Esto es lo que tienes a mano hoy.</p>
        </header>

        <div className={styles.content}>
          <div className={styles.heroGrid}>
            <div className={`${styles.hero} glass`}>
              <div className={styles.eyebrow}>
                <LearningIcon width={14} height={14} /> {continueFormation && continueFormation.progressPct > 0 ? "Continúa" : "Empieza"}
              </div>
              {summary === null ? (
                <p className={styles.muted}>Cargando tu formación…</p>
              ) : continueFormation ? (
                <>
                  <h3 className={styles.heroTitle}>{continueFormation.title}</h3>
                  <p className={styles.muted}>
                    {continueFormation.nextModuleTitle ? `Siguiente: ${continueFormation.nextModuleTitle}` : "Revisa tus módulos"}
                  </p>
                  <div className={styles.progressRow}>
                    <div className="bar-track">
                      <div className="bar-fill" style={{ width: `${continueFormation.progressPct}%` }} />
                    </div>
                    <span className={styles.progressPct}>{continueFormation.progressPct}%</span>
                  </div>
                  <p className={styles.muted}>
                    {continueFormation.completedModules} de {pluralWord(continueFormation.modulesCount, "módulo", "módulos")}
                  </p>
                  <button className="btn-primary" type="button" onClick={() => navigate(`/formacion/${continueFormation.formationId}`)}>
                    <ArrowRightIcon /> {continueFormation.progressPct > 0 ? "Continuar formación" : "Empezar formación"}
                  </button>
                </>
              ) : (
                <>
                  <h3 className={styles.heroTitle}>
                    {totalFormations > 0 ? "Has completado todas tus formaciones" : "Todavía no tienes formaciones"}
                  </h3>
                  <p className={styles.muted}>
                    {totalFormations > 0
                      ? "Buen trabajo. Cuando te asignen una nueva la verás aquí."
                      : "Cuando te asignen una formación activa aparecerá aquí."}
                  </p>
                </>
              )}
            </div>

            <div className={`${styles.resume} glass`}>
              <div className={styles.eyebrow}>
                <ChatIcon width={14} height={14} /> Tu última conversación
              </div>
              {latestConversation === undefined ? (
                <p className={styles.muted}>Cargando…</p>
              ) : latestConversation ? (
                <>
                  <h3 className={styles.resumeTitle}>{latestConversation.title}</h3>
                  <p className={styles.muted}>
                    {latestConversationBot ? `${latestConversationBot} · ` : ""}
                    {latestConversation.dateLabel}
                  </p>
                  <button
                    className="btn-ghost"
                    type="button"
                    onClick={() => navigate("/chat", { state: { conversationId: latestConversation.id } })}
                  >
                    <ArrowRightIcon /> Retomar conversación
                  </button>
                </>
              ) : (
                <>
                  <h3 className={styles.resumeTitle}>Aún no has preguntado nada</h3>
                  <p className={styles.muted}>Resuelve tus dudas con un chatbot.</p>
                  <button className="btn-ghost" type="button" onClick={() => navigate("/chat")}>
                    <ArrowRightIcon /> Ir al chat
                  </button>
                </>
              )}
            </div>
          </div>

          <div className={`${styles.pending} glass`}>
            <div className="panel-head">
              <h2>Pendiente</h2>
            </div>
            {summary === null ? (
              <p className={styles.empty}>Cargando…</p>
            ) : !hasPending ? (
              <p className={styles.empty}>No tienes nada pendiente. 🎉</p>
            ) : (
              <ul className={styles.pendingList}>
                {pendingExams.map((exam) => (
                  <li className={styles.pendingItem} key={exam.moduleId}>
                    <span className={styles.pendingIcon}>
                      <QuizIcon width={16} height={16} />
                    </span>
                    <div className={styles.pendingText}>
                      <div className={styles.pendingTitle}>{exam.moduleTitle}</div>
                      <div className={styles.muted}>
                        {exam.formationTitle} · último intento {exam.lastScore}% (mínimo {exam.passingScore}%)
                      </div>
                    </div>
                    <button className="btn-ghost" type="button" onClick={() => navigate(`/formacion/${exam.formationId}`)}>
                      <ArrowRightIcon /> Reintentar
                    </button>
                  </li>
                ))}
                {otherFormations.map((formation) => (
                  <li className={styles.pendingItem} key={formation.formationId}>
                    <span className={styles.pendingIcon}>
                      <LearningIcon width={16} height={16} />
                    </span>
                    <div className={styles.pendingText}>
                      <div className={styles.pendingTitle}>{formation.title}</div>
                      <div className={styles.muted}>
                        {formation.progressPct > 0 ? `${formation.progressPct}% completado` : "Sin empezar"}
                      </div>
                    </div>
                    <button className="btn-ghost" type="button" onClick={() => navigate(`/formacion/${formation.formationId}`)}>
                      <ArrowRightIcon /> {formation.progressPct > 0 ? "Continuar" : "Empezar"}
                    </button>
                  </li>
                ))}
              </ul>
            )}
          </div>

          <div className={styles.statGrid}>
            <div className={`${styles.statCard} glass`}>
              <div className={styles.statLabel}>Preguntas esta semana</div>
              <div className={styles.statValue}>{summary?.questionsThisWeek ?? 0}</div>
            </div>
            <div className={`${styles.statCard} glass`}>
              <div className={styles.statLabel}>Días activo (últimos 30)</div>
              <div className={styles.statValue}>{summary?.activeDaysLast30 ?? 0}</div>
            </div>
            <div className={`${styles.statCard} glass`}>
              <div className={styles.statLabel}>Formaciones completadas</div>
              <div className={styles.statValue}>
                {completedFormations}
                <span className={styles.statOf}>de {totalFormations}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
