import { useState } from "react";
import Select from "../../components/shared/Select";
import { CheckIcon, CloseIcon, PlusIcon, TrashIcon } from "../../components/shared/icons";
import { useFormations } from "../../shared/contexts/FormationsContext";
import { useToast } from "../../shared/contexts/ToastContext";
import {
  MAX_PASSING_SCORE,
  MIN_PASSING_SCORE,
  type FormationModule,
  type ModuleType,
  type QuizQuestion
} from "../../../modules/formations/domain/Formation";
import GenerateQuizDialog from "./GenerateQuizDialog";
import styles from "./FormationManagePage.module.scss";

interface QuizDraftQuestion {
  question: string;
  options: string[];
  correctIndex: number;
}

function emptyQuestion(): QuizDraftQuestion {
  return { question: "", options: ["", ""], correctIndex: 0 };
}

interface ModuleFormProps {
  formationKey: string;
  module?: FormationModule;
  onClose: () => void;
}

const TYPE_OPTIONS: { value: ModuleType; label: string }[] = [
  { value: "video", label: "Vídeo" },
  { value: "documento", label: "Documento" },
  { value: "ejercicio", label: "Ejercicio" },
  { value: "examen", label: "Examen" }
];

const SUMMARY_LABEL: Record<ModuleType, string> = {
  video: "Resumen del vídeo",
  documento: "Resumen del documento",
  ejercicio: "Instrucciones del ejercicio",
  examen: "Qué valida este examen"
};

export default function ModuleForm({ formationKey, module, onClose }: ModuleFormProps) {
  const [title, setTitle] = useState(module?.title ?? "");
  const [type, setType] = useState<ModuleType>(module?.type ?? "documento");
  const [summary, setSummary] = useState(module?.summary ?? "");
  const [resourceUrl, setResourceUrl] = useState(module?.resourceUrl ?? "");
  const [durationMin, setDurationMin] = useState(module?.durationMin != null ? String(module.durationMin) : "10");
  const [passingScore, setPassingScore] = useState(module?.passingScore != null ? String(module.passingScore) : "");
  const [gated, setGated] = useState(module?.gated ?? true);
  const [quiz, setQuiz] = useState<QuizDraftQuestion[]>(
    module?.type === "examen" && module.quiz && module.quiz.length > 0
      ? module.quiz.map((q) => ({ question: q.question, options: [...q.options], correctIndex: q.correctIndex ?? 0 }))
      : [emptyQuestion()]
  );
  const [titleError, setTitleError] = useState(false);
  const [summaryError, setSummaryError] = useState(false);
  const [urlError, setUrlError] = useState(false);
  const [quizError, setQuizError] = useState(false);
  const [scoreError, setScoreError] = useState(false);
  const [saving, setSaving] = useState(false);
  const [generateOpen, setGenerateOpen] = useState(false);
  const [generated, setGenerated] = useState(false);
  const { addModule, updateModule } = useFormations();
  const { showToast } = useToast();

  function addQuestion() {
    setQuiz((prev) => [...prev, emptyQuestion()]);
  }

  function addGeneratedQuestions(questions: QuizQuestion[]) {
    const drafts: QuizDraftQuestion[] = questions.map((q) => ({ question: q.question, options: [...q.options], correctIndex: q.correctIndex ?? 0 }));
    setQuiz((prev) => {
      const isSingleBlank = prev.length === 1 && !prev[0].question.trim() && prev[0].options.every((o) => !o.trim());
      return isSingleBlank ? drafts : [...prev, ...drafts];
    });
    setQuizError(false);
    setGenerated(true);
    setGenerateOpen(false);
  }

  function removeQuestion(qi: number) {
    setQuiz((prev) => prev.filter((_, i) => i !== qi));
  }

  function updateQuestionText(qi: number, value: string) {
    setQuiz((prev) => prev.map((q, i) => (i === qi ? { ...q, question: value } : q)));
  }

  function addOption(qi: number) {
    setQuiz((prev) => prev.map((q, i) => (i === qi ? { ...q, options: [...q.options, ""] } : q)));
  }

  function removeOption(qi: number, oi: number) {
    setQuiz((prev) =>
      prev.map((q, i) => {
        if (i !== qi) return q;
        const options = q.options.filter((_, o) => o !== oi);
        const correctIndex = q.correctIndex === oi ? 0 : q.correctIndex > oi ? q.correctIndex - 1 : q.correctIndex;
        return { ...q, options, correctIndex };
      })
    );
  }

  function updateOptionText(qi: number, oi: number, value: string) {
    setQuiz((prev) =>
      prev.map((q, i) => (i === qi ? { ...q, options: q.options.map((o, oo) => (oo === oi ? value : o)) } : q))
    );
  }

  function setCorrectOption(qi: number, oi: number) {
    setQuiz((prev) => prev.map((q, i) => (i === qi ? { ...q, correctIndex: oi } : q)));
  }

  async function handleSave() {
    const trimmedTitle = title.trim();
    const trimmedSummary = summary.trim();
    const needsUrl = type === "video" || type === "documento" || type === "ejercicio";
    const needsQuiz = type === "examen";
    const trimmedUrl = resourceUrl.trim();
    const quizValid =
      !needsQuiz ||
      (quiz.length > 0 &&
        quiz.every((q) => q.question.trim().length > 0 && q.options.length >= 2 && q.options.every((o) => o.trim().length > 0)));

    const score = Number(passingScore);
    const scoreValid =
      !needsQuiz || (passingScore.trim() !== "" && Number.isInteger(score) && score >= MIN_PASSING_SCORE && score <= MAX_PASSING_SCORE);

    setTitleError(!trimmedTitle);
    setSummaryError(!trimmedSummary);
    setUrlError(needsUrl && !trimmedUrl);
    setQuizError(needsQuiz && !quizValid);
    setScoreError(!scoreValid);
    if (!trimmedTitle || !trimmedSummary || (needsUrl && !trimmedUrl) || (needsQuiz && !quizValid) || !scoreValid) return;

    const payload =
      type === "examen"
        ? {
            title: trimmedTitle,
            type,
            summary: trimmedSummary,
            gated,
            aiDraft: generated,
            passingScore: score,
            quiz: quiz.map((q) => ({
              question: q.question.trim(),
              options: q.options.map((o) => o.trim()),
              correctIndex: q.correctIndex
            }))
          }
        : {
            title: trimmedTitle,
            type,
            summary: trimmedSummary,
            gated,
            aiDraft: false,
            resourceUrl: trimmedUrl,
            durationMin: type === "video" ? Number(durationMin) || undefined : undefined
          };

    setSaving(true);
    try {
      if (module) {
        await updateModule(formationKey, { ...payload, id: module.id });
        showToast("Módulo actualizado");
      } else {
        await addModule(formationKey, payload);
        showToast(type === "examen" ? "Módulo de examen creado" : "Módulo creado");
      }
      onClose();
    } catch {
      showToast("No se pudieron guardar los cambios — revisá tu conexión e intentá de nuevo");
    } finally {
      setSaving(false);
    }
  }

  return (
    <div className={styles.moduleForm}>
      <div className="form-field">
        <label htmlFor="module-type">Tipo de paso</label>
        <Select id="module-type" value={type} onChange={setType} options={TYPE_OPTIONS} ariaLabel="Tipo de paso" fullWidth />
      </div>

      <div className="form-field">
        <label htmlFor="module-title">Título</label>
        <input
          id="module-title"
          type="text"
          className={`form-input${titleError ? " field-error" : ""}`}
          placeholder="p. ej. Vídeo: primeros pasos con el repositorio"
          value={title}
          onChange={(e) => {
            setTitle(e.target.value);
            if (titleError) setTitleError(false);
          }}
          autoFocus
        />
      </div>

      <div className="form-field">
        <label htmlFor="module-summary">{SUMMARY_LABEL[type]}</label>
        <textarea
          id="module-summary"
          className={`form-input${summaryError ? " field-error" : ""}`}
          rows={3}
          value={summary}
          onChange={(e) => {
            setSummary(e.target.value);
            if (summaryError) setSummaryError(false);
          }}
        />
      </div>

      {(type === "video" || type === "documento" || type === "ejercicio") && (
        <div className="form-field">
          <label htmlFor="module-url">
            {type === "video" ? "Enlace al vídeo" : type === "documento" ? "Enlace al documento" : "Enlace al ejercicio/repositorio"}
          </label>
          <input
            id="module-url"
            type="text"
            className={`form-input${urlError ? " field-error" : ""}`}
            placeholder="https://…"
            value={resourceUrl}
            onChange={(e) => {
              setResourceUrl(e.target.value);
              if (urlError) setUrlError(false);
            }}
          />
        </div>
      )}

      {type === "video" && (
        <div className="form-field">
          <label htmlFor="module-duration">Duración (min)</label>
          <input
            id="module-duration"
            type="number"
            min={1}
            className="form-input"
            value={durationMin}
            onChange={(e) => setDurationMin(e.target.value)}
          />
        </div>
      )}

      {type === "examen" && (
        <>
          <div className="form-field">
            <label htmlFor="module-score">Nota mínima para aprobar (%)</label>
            <input
              id="module-score"
              type="number"
              placeholder="p. ej. 70"
              min={MIN_PASSING_SCORE}
              max={MAX_PASSING_SCORE}
              className={`form-input${scoreError ? " field-error" : ""}`}
              value={passingScore}
              onChange={(e) => {
                setPassingScore(e.target.value);
                if (scoreError) setScoreError(false);
              }}
            />
          </div>

          <div className="form-field">
            <label>Preguntas del examen</label>
            {quiz.map((q, qi) => (
              <div key={qi} className={styles.questionCard}>
                <div className={styles.questionHead}>
                  <span className={styles.questionTitle}>Pregunta {qi + 1}</span>
                  <button
                    className={`btn-ghost danger ${styles.questionRemove}`}
                    type="button"
                    onClick={() => removeQuestion(qi)}
                    disabled={quiz.length <= 1}
                  >
                    <TrashIcon width={12} height={12} /> Quitar pregunta
                  </button>
                </div>
                <input
                  type="text"
                  className={`form-input ${styles.questionText}${quizError && !q.question.trim() ? " field-error" : ""}`}
                  placeholder="Texto de la pregunta"
                  value={q.question}
                  onChange={(e) => updateQuestionText(qi, e.target.value)}
                />
                <div className={styles.optionList}>
                  {q.options.map((opt, oi) => (
                    <div key={oi} className={styles.optionRow}>
                      <input
                        type="radio"
                        name={`quiz-${qi}-correct`}
                        checked={q.correctIndex === oi}
                        onChange={() => setCorrectOption(qi, oi)}
                        aria-label={`Marcar la opción ${oi + 1} como correcta`}
                      />
                      <input
                        type="text"
                        className={`form-input ${styles.optionText}${quizError && !opt.trim() ? " field-error" : ""}`}
                        placeholder={`Opción ${oi + 1}`}
                        value={opt}
                        onChange={(e) => updateOptionText(qi, oi, e.target.value)}
                      />
                      <button
                        className={`btn-ghost ${styles.optionRemove}`}
                        type="button"
                        onClick={() => removeOption(qi, oi)}
                        disabled={q.options.length <= 2}
                        aria-label={`Quitar la opción ${oi + 1}`}
                      >
                        <CloseIcon width={11} height={11} />
                      </button>
                    </div>
                  ))}
                </div>
                <button
                  className={`btn-ghost ${styles.optionAdd}`}
                  type="button"
                  onClick={() => addOption(qi)}
                >
                  <PlusIcon width={12} height={12} /> Añadir opción
                </button>
              </div>
            ))}
            <div className={styles.quizActions}>
              <button className="btn-ghost" type="button" onClick={addQuestion}>
                <PlusIcon /> Añadir pregunta
              </button>
              <button className="btn-ghost" type="button" onClick={() => setGenerateOpen(true)}>
                <PlusIcon /> Generar preguntas con IA
              </button>
            </div>
            {quizError && (
              <p className={styles.quizError}>
                Cada pregunta necesita su texto, al menos 2 opciones completas y una marcada como correcta.
              </p>
            )}
            <GenerateQuizDialog
              open={generateOpen}
              formationKey={formationKey}
              topic={[title.trim(), summary.trim()].filter(Boolean).join(". ")}
              onClose={() => setGenerateOpen(false)}
              onGenerated={addGeneratedQuestions}
            />
          </div>
        </>
      )}

      <div className={`config-row ${styles.gateRow}`}>
        <div className="config-row-text">
          <div className="title">Bloquea el siguiente módulo</div>
          <div className="desc">Los juniors no podrán acceder al siguiente módulo hasta completar este.</div>
        </div>
        <button className="switch" role="switch" aria-checked={gated} aria-label="Bloquea el siguiente módulo" type="button" onClick={() => setGated((g) => !g)} />
      </div>

      <div className={styles.moduleFormActions}>
        <button className="btn-ghost" type="button" onClick={onClose} disabled={saving}>
          <CloseIcon /> Cancelar
        </button>
        <button className="btn-primary" type="button" onClick={handleSave} disabled={saving}>
          {module ? (
            <>
              <CheckIcon /> {saving ? "Guardando…" : "Guardar cambios"}
            </>
          ) : (
            <>
              <PlusIcon /> {saving ? "Creando…" : "Crear módulo"}
            </>
          )}
        </button>
      </div>
    </div>
  );
}
