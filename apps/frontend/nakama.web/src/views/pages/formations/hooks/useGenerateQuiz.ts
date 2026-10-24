import { useEffect, useState } from "react";
import type { ILlmProvidersRepository } from "../../../../modules/llmProviders/domain/ILlmProvidersRepository";
import type { LlmProviderConfig, LlmProviderKind } from "../../../../modules/llmProviders/domain/LlmProviderConfig";
import { MAX_GENERATED_QUESTIONS, type QuizQuestion } from "../../../../modules/formations/domain/Formation";
import type { IFormationsRepository } from "../../../../modules/formations/domain/IFormationsRepository";
import { Services } from "../../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../../shared/infrastructure/injection/useInjection";
import { useFormations } from "../../../shared/contexts/FormationsContext";
import { useToast } from "../../../shared/contexts/ToastContext";

const DEFAULT_QUESTION_COUNT = "3";

const PROVIDER_LABEL: Record<LlmProviderKind, string> = {
  anthropic: "Anthropic (Claude)",
  openai: "OpenAI",
  local: "Modelo local"
};

export function useGenerateQuiz(formationKey: string, topic: string, onGenerated: (questions: QuizQuestion[]) => void) {
  const { formations } = useFormations();
  const { showToast } = useToast();
  const formationsRepo = useInjection<IFormationsRepository>(Services.FormationsRepository.id);
  const llmProvidersRepo = useInjection<ILlmProvidersRepository>(Services.LlmProvidersRepository.id);

  const docs = formations[formationKey]?.docs ?? [];

  const [providers, setProviders] = useState<LlmProviderConfig[]>([]);
  const [documentIds, setDocumentIds] = useState<string[]>(docs.map((doc) => doc.id));
  const [provider, setProvider] = useState<LlmProviderKind | "">("");
  const [model, setModel] = useState("");
  const [count, setCount] = useState(DEFAULT_QUESTION_COUNT);
  const [generating, setGenerating] = useState(false);

  useEffect(() => {
    let cancelled = false;
    llmProvidersRepo.FindAll().then((rows) => {
      if (cancelled) return;
      const usable = rows.filter((row) => row.configured && row.enabled && row.models.length > 0);
      setProviders(usable);
      setProvider((current) => current || (usable[0]?.provider ?? ""));
      setModel((current) => current || (usable[0]?.models[0]?.id ?? ""));
    });
    return () => {
      cancelled = true;
    };
  }, [llmProvidersRepo]);

  const selectedProvider = providers.find((row) => row.provider === provider);
  const parsedCount = Number(count);
  const countValid = Number.isInteger(parsedCount) && parsedCount >= 1 && parsedCount <= MAX_GENERATED_QUESTIONS;
  const canGenerate = !generating && documentIds.length > 0 && model !== "" && countValid;

  function toggleDocument(id: string) {
    setDocumentIds((prev) => (prev.includes(id) ? prev.filter((documentId) => documentId !== id) : [...prev, id]));
  }

  function selectProvider(next: LlmProviderKind) {
    setProvider(next);
    setModel(providers.find((row) => row.provider === next)?.models[0]?.id ?? "");
  }

  async function generate() {
    if (!canGenerate || provider === "") return;
    setGenerating(true);
    try {
      const questions = await formationsRepo.GenerateQuizQuestions({
        formationId: formationKey,
        documentIds,
        provider,
        model,
        questionCount: parsedCount,
        topic
      });
      onGenerated(questions);
    } catch (error) {
      const detail = error && typeof error === "object" && "detail" in error ? String((error as { detail?: unknown }).detail) : null;
      showToast(detail ?? "No se pudieron generar las preguntas.");
    } finally {
      setGenerating(false);
    }
  }

  return {
    docs,
    providerOptions: providers.map((row) => ({ value: row.provider, label: PROVIDER_LABEL[row.provider] })),
    modelOptions: (selectedProvider?.models ?? []).map((option) => ({ value: option.id, label: option.label })),
    documentIds,
    provider,
    model,
    count,
    countValid,
    generating,
    canGenerate,
    setModel,
    setCount,
    toggleDocument,
    selectProvider,
    generate
  };
}
