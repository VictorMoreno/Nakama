import { useState } from "react";
import { useNavigate } from "react-router-dom";
import type { FormationLifecycleStatus } from "../../../../modules/formations/domain/Formation";
import { useChatbots } from "../../../shared/contexts/ChatbotsContext";
import { useFormations } from "../../../shared/contexts/FormationsContext";
import { useToast } from "../../../shared/contexts/ToastContext";

export function useCreateFormation() {
  const navigate = useNavigate();
  const { activeChatbots } = useChatbots();
  const { createFormation } = useFormations();
  const { showToast } = useToast();

  const [title, setTitle] = useState("");
  const [audience, setAudience] = useState("");
  const [description, setDescription] = useState("");
  const [status, setStatus] = useState<FormationLifecycleStatus>("borrador");
  const [linkedChatbotKey, setLinkedChatbotKey] = useState("");
  const [iconUrl, setIconUrl] = useState<string | undefined>(undefined);
  const [titleError, setTitleError] = useState(false);

  async function handleConfirm() {
    const trimmed = title.trim();
    if (!trimmed) {
      setTitleError(true);
      return;
    }
    const key = await createFormation({
      title: trimmed,
      audience: audience.trim(),
      description,
      status,
      linkedChatbotKey: linkedChatbotKey || undefined,
      iconUrl
    });
    showToast("Currículo creado");
    navigate(`/formacion/${key}`);
  }

  return {
    activeChatbots,
    title,
    setTitle,
    audience,
    setAudience,
    description,
    setDescription,
    status,
    setStatus,
    linkedChatbotKey,
    setLinkedChatbotKey,
    iconUrl,
    setIconUrl,
    titleError,
    setTitleError,
    handleConfirm
  };
}
