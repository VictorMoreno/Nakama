import { useState } from "react";
import type { Formation } from "../../../../modules/formations/domain/Formation";
import { useFormations } from "../../../shared/contexts/FormationsContext";
import { useToast } from "../../../shared/contexts/ToastContext";

export function useFormationDetails(formation: Formation) {
  const { updateDetails } = useFormations();
  const { showToast } = useToast();
  const [title, setTitle] = useState(formation.title);
  const [audience, setAudience] = useState(formation.audience);
  const [description, setDescription] = useState(formation.description);
  const [titleError, setTitleError] = useState(false);
  const [audienceError, setAudienceError] = useState(false);
  const [descriptionError, setDescriptionError] = useState(false);
  const [saving, setSaving] = useState(false);

  const dirty =
    title.trim() !== formation.title || audience.trim() !== formation.audience || description.trim() !== formation.description;

  function changeTitle(value: string) {
    setTitle(value);
    if (titleError) setTitleError(false);
  }

  function changeAudience(value: string) {
    setAudience(value);
    if (audienceError) setAudienceError(false);
  }

  function changeDescription(value: string) {
    setDescription(value);
    if (descriptionError) setDescriptionError(false);
  }

  async function handleSave() {
    const nextTitle = title.trim();
    const nextAudience = audience.trim();
    const nextDescription = description.trim();
    if (!nextTitle || !nextAudience || !nextDescription) {
      setTitleError(!nextTitle);
      setAudienceError(!nextAudience);
      setDescriptionError(!nextDescription);
      return;
    }
    setSaving(true);
    try {
      await updateDetails(formation.id, nextTitle, nextAudience, nextDescription);
      showToast("Datos de la formación actualizados");
    } catch {
      showToast("No se pudieron guardar los datos de la formación");
    } finally {
      setSaving(false);
    }
  }

  return {
    title,
    audience,
    description,
    titleError,
    audienceError,
    descriptionError,
    saving,
    dirty,
    changeTitle,
    changeAudience,
    changeDescription,
    handleSave
  };
}
