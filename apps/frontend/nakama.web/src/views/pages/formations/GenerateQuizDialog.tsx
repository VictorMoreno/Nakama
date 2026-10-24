import Dialog from "../../components/shared/Dialog";
import Select from "../../components/shared/Select";
import { CloseIcon } from "../../components/shared/icons";
import { MAX_GENERATED_QUESTIONS, type QuizQuestion } from "../../../modules/formations/domain/Formation";
import { useGenerateQuiz } from "./hooks/useGenerateQuiz";
import styles from "./GenerateQuizDialog.module.scss";

interface GenerateQuizDialogProps {
  open: boolean;
  formationKey: string;
  topic: string;
  onClose: () => void;
  onGenerated: (questions: QuizQuestion[]) => void;
}

export default function GenerateQuizDialog({ open, formationKey, topic, onClose, onGenerated }: GenerateQuizDialogProps) {
  return (
    <Dialog open={open} onClose={onClose} title="Generar preguntas con IA">
      <GenerateQuizForm formationKey={formationKey} topic={topic} onClose={onClose} onGenerated={onGenerated} />
    </Dialog>
  );
}

interface GenerateQuizFormProps {
  formationKey: string;
  topic: string;
  onClose: () => void;
  onGenerated: (questions: QuizQuestion[]) => void;
}

function GenerateQuizForm({ formationKey, topic, onClose, onGenerated }: GenerateQuizFormProps) {
  const quiz = useGenerateQuiz(formationKey, topic, onGenerated);

  if (quiz.docs.length === 0) {
    return (
      <>
        <p>
          Esta formación todavía no tiene documentos. Súbelos en la pestaña «Documentos» de la formación para poder generar preguntas a
          partir de ellos.
        </p>
        <div className="modal-actions">
          <button className="btn-ghost" type="button" onClick={onClose}>
            <CloseIcon /> Cerrar
          </button>
        </div>
      </>
    );
  }

  return (
    <>
      <p>La IA redacta un borrador de preguntas tipo test a partir de los documentos que elijas. Podrás revisarlo y editarlo antes de guardar el módulo.</p>

      <div className="form-field">
        <label>Documentos</label>
        <div className={styles.documents}>
          {quiz.docs.map((doc) => (
            <label className={styles.document} key={doc.id}>
              <input type="checkbox" checked={quiz.documentIds.includes(doc.id)} onChange={() => quiz.toggleDocument(doc.id)} />
              <span>{doc.name}</span>
            </label>
          ))}
        </div>
      </div>

      {quiz.providerOptions.length === 0 ? (
        <p className="form-hint">No hay ningún proveedor de IA activo con modelos. Configúralo en Proveedores de IA.</p>
      ) : (
        <div className={styles.row}>
          <div className="form-field">
            <label>Proveedor</label>
            <Select
              value={quiz.provider === "" ? quiz.providerOptions[0].value : quiz.provider}
              onChange={quiz.selectProvider}
              options={quiz.providerOptions}
              ariaLabel="Proveedor"
              fullWidth
            />
          </div>
          <div className="form-field">
            <label>Modelo</label>
            <Select value={quiz.model} onChange={quiz.setModel} options={quiz.modelOptions} ariaLabel="Modelo" fullWidth />
          </div>
        </div>
      )}

      <div className="form-field">
        <label htmlFor="quiz-count">Número de preguntas (máx. {MAX_GENERATED_QUESTIONS})</label>
        <input
          id="quiz-count"
          type="number"
          min={1}
          max={MAX_GENERATED_QUESTIONS}
          className={`form-input${quiz.countValid ? "" : " field-error"}`}
          value={quiz.count}
          onChange={(e) => quiz.setCount(e.target.value)}
        />
      </div>

      <div className="modal-actions">
        <button className="btn-ghost" type="button" onClick={onClose} disabled={quiz.generating}>
          <CloseIcon /> Cancelar
        </button>
        <button className="btn-primary" type="button" onClick={quiz.generate} disabled={!quiz.canGenerate}>
          {quiz.generating ? "Generando…" : "Generar"}
        </button>
      </div>
    </>
  );
}
