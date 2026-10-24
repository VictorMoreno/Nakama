import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useChatbots } from "../../../shared/contexts/ChatbotsContext";
import { useToast } from "../../../shared/contexts/ToastContext";
import type { ChatbotLifecycleStatus, LlmProvider } from "../../../../modules/chatbots/domain/Chatbot";
import type { ILlmProvidersRepository } from "../../../../modules/llmProviders/domain/ILlmProvidersRepository";
import type { LlmProviderConfig } from "../../../../modules/llmProviders/domain/LlmProviderConfig";
import { Services } from "../../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../../shared/infrastructure/injection/useInjection";

export function useCreateChatbot() {
  const navigate = useNavigate();
  const { createChatbot } = useChatbots();
  const { showToast } = useToast();
  const llmProvidersRepo = useInjection<ILlmProvidersRepository>(Services.LlmProvidersRepository.id);
  const [providers, setProviders] = useState<LlmProviderConfig[]>([]);
  const [model, setModel] = useState("");

  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [status, setStatus] = useState<ChatbotLifecycleStatus>("borrador");
  const [socratic, setSocratic] = useState(false);
  const [explicaMas, setExplicaMas] = useState(true);
  const [provider, setProvider] = useState<LlmProvider>("anthropic");
  const [iconUrl, setIconUrl] = useState<string | undefined>(undefined);
  const [nameError, setNameError] = useState(false);
  const [descriptionError, setDescriptionError] = useState(false);

  useEffect(() => {
    let cancelled = false;
    llmProvidersRepo.FindAll().then((rows) => {
      if (!cancelled) setProviders(rows);
    });
    return () => {
      cancelled = true;
    };
  }, [llmProvidersRepo]);

  const modelOptions = (providers.find((p) => p.provider === provider)?.models ?? []).map((m) => ({
    value: m.id,
    label: m.label
  }));
  const selectedModel = modelOptions.some((o) => o.value === model) ? model : (modelOptions[0]?.value ?? "");

  function handleProviderChange(next: LlmProvider) {
    setProvider(next);
    setModel("");
  }

  async function handleConfirm() {
    const trimmed = name.trim();
    if (!trimmed || !description.trim()) {
      setNameError(!trimmed);
      setDescriptionError(!description.trim());
      return;
    }
    if (!selectedModel) {
      showToast("Elige un modelo de lenguaje. Si no hay ninguno, regístralo en Proveedores de IA.");
      return;
    }
    const key = await createChatbot({
      name: trimmed,
      description,
      status,
      settings: { socratic, explicaMas, provider, model: selectedModel },
      iconUrl
    });
    showToast("Chatbot creado");
    navigate(`/chatbots/${key}`);
  }

  return {
    navigate,
    name,
    setName,
    description,
    setDescription,
    status,
    setStatus,
    socratic,
    setSocratic,
    explicaMas,
    setExplicaMas,
    provider,
    handleProviderChange,
    modelOptions,
    selectedModel,
    setModel,
    iconUrl,
    setIconUrl,
    nameError,
    setNameError,
    descriptionError,
    setDescriptionError,
    handleConfirm
  };
}
