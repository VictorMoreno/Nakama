import { useState } from "react";
import { useChatbots } from "../../../../shared/contexts/ChatbotsContext";
import { useToast } from "../../../../shared/contexts/ToastContext";
import type { Chatbot } from "../../../../../modules/chatbots/domain/Chatbot";

export function usePermissionsTab(bot: Chatbot) {
  const { removeTeamMember } = useChatbots();
  const { showToast } = useToast();
  const [inviteOpen, setInviteOpen] = useState(false);
  const [removeIndex, setRemoveIndex] = useState<number | null>(null);
  const target = removeIndex != null ? bot.team[removeIndex] : null;

  function handleRemoveConfirm() {
    if (removeIndex != null) {
      removeTeamMember(bot.id, removeIndex);
      showToast("Acceso actualizado");
    }
    setRemoveIndex(null);
  }

  return { inviteOpen, setInviteOpen, removeIndex, setRemoveIndex, target, handleRemoveConfirm };
}
