import { useEffect, useState } from "react";
import type { Chatbot, LlmProvider } from "../../../../../modules/chatbots/domain/Chatbot";
import type { ILlmProvidersRepository } from "../../../../../modules/llmProviders/domain/ILlmProvidersRepository";
import type { LlmProviderConfig } from "../../../../../modules/llmProviders/domain/LlmProviderConfig";
import { Services } from "../../../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../../../shared/infrastructure/injection/useInjection";
import { useChatbots } from "../../../../shared/contexts/ChatbotsContext";
import { useToast } from "../../../../shared/contexts/ToastContext";

export function useGeneralTab(bot: Chatbot) {
  const { updateSettings, updateIcon, updateDetails } = useChatbots();
  const { showToast } = useToast();
  const llmProvidersRepo = useInjection<ILlmProvidersRepository>(Services.LlmProvidersRepository.id);
  const [providers, setProviders] = useState<LlmProviderConfig[]>([]);
  const [label, setLabel] = useState(bot.label);
  const [description, setDescription] = useState(bot.description);
  const [labelError, setLabelError] = useState(false);
  const [descriptionError, setDescriptionError] = useState(false);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    let cancelled = false;
    llmProvidersRepo.FindAll().then((rows) => {
      if (!cancelled) setProviders(rows);
    });
    return () => {
      cancelled = true;
    };
  }, [llmProvidersRepo]);

  const selectedProvider = providers.find((p) => p.provider === bot.settings.provider);
  const modelOptions = (selectedProvider?.models ?? []).map((m) => ({ value: m.id, label: m.label }));
  const dirty = label.trim() !== bot.label || description.trim() !== bot.description;

  function changeLabel(value: string) {
    setLabel(value);
    if (labelError) setLabelError(false);
  }

  function changeDescription(value: string) {
    setDescription(value);
    if (descriptionError) setDescriptionError(false);
  }

  function changeProvider(provider: LlmProvider) {
    const firstModel = providers.find((p) => p.provider === provider)?.models[0]?.id ?? "";
    updateSettings(bot.id, { provider, model: firstModel });
  }

  async function handleSaveDetails() {
    const nextLabel = label.trim();
    const nextDescription = description.trim();
    if (!nextLabel || !nextDescription) {
      setLabelError(!nextLabel);
      setDescriptionError(!nextDescription);
      return;
    }
    setSaving(true);
    try {
      await updateDetails(bot.id, nextLabel, nextDescription);
      showToast("Datos del chatbot actualizados");
    } catch {
      showToast("No se pudieron guardar los datos del chatbot");
    } finally {
      setSaving(false);
    }
  }

  return {
    updateSettings,
    updateIcon,
    modelOptions,
    label,
    description,
    labelError,
    descriptionError,
    saving,
    dirty,
    changeLabel,
    changeDescription,
    changeProvider,
    handleSaveDetails
  };
}
