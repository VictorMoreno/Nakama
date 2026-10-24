import type { FormationModule, LearnerProgress } from "../../../modules/formations/domain/Formation";
import { pluralWord } from "../../../lib/text";
import ExamAttemptsList from "./ExamAttemptsList";
import styles from "./TeamMemberExams.module.scss";

interface TeamMemberExamsProps {
  modules: FormationModule[];
  progress: LearnerProgress | undefined;
  loading: boolean;
  failed: boolean;
}

export default function TeamMemberExams({ modules, progress, loading, failed }: TeamMemberExamsProps) {
  const exams = modules.filter((module) => module.type === "examen");

  if (failed) return <div className={styles.panel}>No se pudieron cargar los exámenes — vuelve a abrir la pestaña para reintentarlo.</div>;
  if (loading) return <div className={styles.panel}>Cargando exámenes…</div>;
  if (exams.length === 0) return <div className={styles.panel}>Esta formación no tiene exámenes.</div>;

  return (
    <div className={styles.panel}>
      {exams.map((exam) => {
        const attempts = (progress?.examAttempts ?? []).filter((attempt) => attempt.moduleId === exam.id);
        const passedScore = progress?.examScores[exam.id];
        const summary =
          attempts.length === 0
            ? "Sin intentos todavía"
            : `${passedScore !== undefined ? `Aprobado con ${passedScore}%` : "Sin aprobar"} · ${pluralWord(attempts.length, "intento", "intentos")}`;

        return (
          <div className={styles.exam} key={exam.id}>
            <div className={styles.examHead}>
              <span className={styles.examTitle}>{exam.title}</span>
              <span className={styles.examSummary}>{summary}</span>
            </div>
            {attempts.length > 0 && <ExamAttemptsList attempts={attempts} />}
          </div>
        );
      })}
    </div>
  );
}
