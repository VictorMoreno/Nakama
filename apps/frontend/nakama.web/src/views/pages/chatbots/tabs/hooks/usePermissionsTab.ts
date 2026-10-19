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

  async function handleRemoveConfirm() {
    const userId = removeUserId;
    setRemoveUserId(null);
    if (userId == null) return;
    try {
      await removeTeamMember(bot.id, userId);
      showToast("Acceso actualizado");
    } catch {
      showToast("No se pudo quitar el acceso");
    }
  }

  return { inviteOpen, setInviteOpen, removeUserId, setRemoveUserId, target, handleRemoveConfirm };
}
