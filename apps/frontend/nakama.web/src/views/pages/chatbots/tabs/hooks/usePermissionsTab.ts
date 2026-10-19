import { useState } from "react";
import { useChatbots } from "../../../../shared/contexts/ChatbotsContext";
import { useToast } from "../../../../shared/contexts/ToastContext";
import type { Chatbot } from "../../../../../modules/chatbots/domain/Chatbot";

export function usePermissionsTab(bot: Chatbot) {
  const { removeTeamMember } = useChatbots();
  const { showToast } = useToast();
  const [inviteOpen, setInviteOpen] = useState(false);
  const [removeUserId, setRemoveUserId] = useState<string | null>(null);
  const target = removeUserId != null ? bot.team.find((member) => member.userId === removeUserId) : null;

  function handleRemoveConfirm() {
    if (removeUserId != null) {
      removeTeamMember(bot.id, removeUserId);
      showToast("Acceso actualizado");
    }
    setRemoveUserId(null);
  }

  return { inviteOpen, setInviteOpen, removeUserId, setRemoveUserId, target, handleRemoveConfirm };
}
