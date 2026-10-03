import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useChatbots } from "../../../shared/contexts/ChatbotsContext";
import { useToast } from "../../../shared/contexts/ToastContext";
import type { ChatbotLifecycleStatus, LlmProvider } from "../../../../modules/chatbots/domain/Chatbot";

export function useCreateChatbot() {
  const navigate = useNavigate();
  const { createChatbot } = useChatbots();
  const { showToast } = useToast();

  const [name, setName] = useState("");
  const [general, setGeneral] = useState(false);
  const [dept, setDept] = useState("");
  const [description, setDescription] = useState("");
  const [status, setStatus] = useState<ChatbotLifecycleStatus>("borrador");
  const [socratic, setSocratic] = useState(false);
  const [explicaMas, setExplicaMas] = useState(true);
  const [provider, setProvider] = useState<LlmProvider>("anthropic");
  const [iconUrl, setIconUrl] = useState<string | undefined>(undefined);
  const [nameError, setNameError] = useState(false);

  async function handleConfirm() {
    const trimmed = name.trim();
    if (!trimmed) {
      setNameError(true);
      return;
    }
    const key = await createChatbot({
      name: trimmed,
      dept: dept.trim(),
      general,
      description,
      status,
      settings: { socratic, explicaMas, provider, model: "" },
      iconUrl
    });
    showToast("Chatbot creado");
    navigate(`/chatbots/${key}`);
  }

  return {
    navigate,
    name,
    setName,
    general,
    setGeneral,
    dept,
    setDept,
    description,
    setDescription,
    status,
    setStatus,
    socratic,
    setSocratic,
    explicaMas,
    setExplicaMas,
    provider,
    setProvider,
    iconUrl,
    setIconUrl,
    nameError,
    setNameError,
    handleConfirm
  };
}
