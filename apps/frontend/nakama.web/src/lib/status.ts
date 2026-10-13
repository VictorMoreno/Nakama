import type { Chatbot, ChatbotLifecycleStatus } from "../modules/chatbots/domain/Chatbot";
import type { Formation, FormationLifecycleStatus } from "../modules/formations/domain/Formation";

/**
 * Single source of truth for the "estado" shown as the LED dot on cards and as the
 * status-tag in detail views — both must always agree, so both read from here instead
 * of computing their own condition inline. The status itself is configured by the admin
 * on each chatbot/currículo (see GeneralTab / FormationManagePage), not inferred.
 */
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
