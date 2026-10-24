import { formatDateTime } from "../../../shared/infrastructure/formatDate";
import type { ExamAttempt } from "../../../modules/formations/domain/Formation";
import styles from "./ExamAttemptsList.module.scss";

interface ExamAttemptsListProps {
  attempts: ExamAttempt[];
}

export default function ExamAttemptsList({ attempts }: ExamAttemptsListProps) {
  return (
    <div className={styles.tableWrap}>
      <table className={styles.table}>
        <thead>
          <tr>
            <th className={styles.th}>Intento</th>
            <th className={styles.th}>Fecha</th>
            <th className={styles.th}>Nota</th>
            <th className={styles.th}>Resultado</th>
          </tr>
        </thead>
        <tbody>
          {attempts.map((attempt, index) => (
            <tr key={`${attempt.attemptedAt}-${index}`}>
              <td className={styles.td}>{index + 1}</td>
              <td className={styles.td}>{formatDateTime(attempt.attemptedAt)}</td>
              <td className={`${styles.td} ${styles.score}`}>{attempt.score}%</td>
              <td className={styles.td}>
                <span className={`${styles.result} ${attempt.passed ? styles.passed : styles.failed}`}>
                  {attempt.passed ? "Aprobado" : "No superado"}
                </span>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
