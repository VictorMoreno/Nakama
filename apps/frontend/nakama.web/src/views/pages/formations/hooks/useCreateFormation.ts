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
      iconUrl
    });
    showToast("Formación creada");
    // Straight into manage mode, not the plain execute view - right after creating it, the next
    // thing an admin does is add modules, which only the gestionar route exposes.
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
    handleConfirm
  };
}
