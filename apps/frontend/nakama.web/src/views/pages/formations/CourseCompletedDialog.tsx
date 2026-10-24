import type { CSSProperties } from "react";
import Dialog from "../../components/shared/Dialog";
import { CheckIcon } from "../../components/shared/icons";
import styles from "./CourseCompletedDialog.module.scss";

interface CourseCompletedDialogProps {
  open: boolean;
  formationTitle: string;
  onClose: () => void;
}

const CONFETTI_COLORS = ["var(--accent)", "var(--cyan)", "var(--gold)", "var(--danger)"];
const CONFETTI_PIECES = Array.from({ length: 28 }, (_, index) => index);

export default function CourseCompletedDialog({ open, formationTitle, onClose }: CourseCompletedDialogProps) {
  return (
    <Dialog open={open} onClose={onClose} title="¡Formación completada!">
      <div className={styles.celebration}>
        <div className={styles.confetti} aria-hidden="true">
          {CONFETTI_PIECES.map((index) => (
            <span
              key={index}
              className={styles.piece}
              style={
                {
                  left: `${(index * 37) % 100}%`,
                  background: CONFETTI_COLORS[index % CONFETTI_COLORS.length],
                  animationDelay: `${(index % 7) * 0.14}s`,
                  animationDuration: `${2.4 + (index % 5) * 0.3}s`
                } as CSSProperties
              }
            />
          ))}
        </div>
        <div className={styles.badge}>
          <CheckIcon width={34} height={34} />
        </div>
        <p className={styles.message}>
          Has terminado <strong>{formationTitle}</strong>. Has completado todos los módulos: enhorabuena por el trabajo.
        </p>
      </div>
      <div className="modal-actions">
        <button className="btn-primary" type="button" onClick={onClose}>
          <CheckIcon /> Genial
        </button>
      </div>
    </Dialog>
  );
}
