import { useState } from "react";
import { useNavigate } from "react-router-dom";
import type { FormationLifecycleStatus } from "../../../../modules/formations/domain/Formation";
import { useFormations } from "../../../shared/contexts/FormationsContext";
import { useToast } from "../../../shared/contexts/ToastContext";

export function useCreateFormation() {
  const navigate = useNavigate();
  const { createFormation } = useFormations();
  const { showToast } = useToast();

  const [title, setTitle] = useState("");
  const [audience, setAudience] = useState("");
  const [description, setDescription] = useState("");
  const [status, setStatus] = useState<FormationLifecycleStatus>("borrador");
  const [iconUrl, setIconUrl] = useState<string | undefined>(undefined);
  const [titleError, setTitleError] = useState(false);
  const [audienceError, setAudienceError] = useState(false);
  const [descriptionError, setDescriptionError] = useState(false);

  async function handleConfirm() {
    const trimmed = title.trim();
    const trimmedAudience = audience.trim();
    const trimmedDescription = description.trim();
    if (!trimmed || !trimmedAudience || !trimmedDescription) {
      setTitleError(!trimmed);
      setAudienceError(!trimmedAudience);
      setDescriptionError(!trimmedDescription);
      return;
    }
    const key = await createFormation({
      title: trimmed,
      audience: trimmedAudience,
      description: trimmedDescription,
      status,
      iconUrl
    });
    showToast("Formación creada");
    navigate(`/formacion/${key}/gestionar`);
  }

  return {
    title,
    setTitle,
    audience,
    setAudience,
    description,
    setDescription,
    status,
    setStatus,
    iconUrl,
    setIconUrl,
    titleError,
    setTitleError,
    audienceError,
    setAudienceError,
    descriptionError,
    setDescriptionError,
    handleConfirm
  };
}
