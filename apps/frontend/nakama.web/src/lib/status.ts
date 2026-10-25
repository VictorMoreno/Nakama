import type { Chatbot, ChatbotLifecycleStatus } from "../modules/chatbots/domain/Chatbot";
import type { Formation, FormationLifecycleStatus } from "../modules/formations/domain/Formation";

export type StatusLevel = "ok" | "pending" | "muted" | "danger";

interface EntityStatus {
  level: StatusLevel;
  label: string;
}

export const CHATBOT_STATUS_META: Record<ChatbotLifecycleStatus, EntityStatus> = {
  activo: { level: "ok", label: "Activo" },
  inactivo: { level: "danger", label: "Inactivo" },
  borrador: { level: "muted", label: "Borrador" }
};

export const FORMATION_STATUS_META: Record<FormationLifecycleStatus, EntityStatus> = {
  activo: { level: "ok", label: "Activo" },
  inactivo: { level: "danger", label: "Inactivo" },
  borrador: { level: "muted", label: "Borrador" }
};

export function getChatbotStatus(bot: Chatbot): EntityStatus {
  return CHATBOT_STATUS_META[bot.status];
}

export function getFormationStatus(formation: Formation): EntityStatus {
  return FORMATION_STATUS_META[formation.status];
}
