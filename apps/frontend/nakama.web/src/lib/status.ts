import type { Chatbot, ChatbotLifecycleStatus } from "../types/chatbot";
import type { Curriculum, CurriculumLifecycleStatus } from "../types/curriculum";

/**
 * Single source of truth for the "estado" shown as the LED dot on cards and as the
 * status-tag in detail views — both must always agree, so both read from here instead
 * of computing their own condition inline. The status itself is configured by the admin
 * on each chatbot/currículo (see GeneralTab / CurriculumDetailView), not inferred.
 */
export type StatusLevel = "ok" | "pending" | "muted" | "danger";

export interface EntityStatus {
  level: StatusLevel;
  label: string;
}

export const CHATBOT_STATUS_META: Record<ChatbotLifecycleStatus, EntityStatus> = {
  activo: { level: "ok", label: "Activo" },
  inactivo: { level: "danger", label: "Inactivo" },
  borrador: { level: "muted", label: "Borrador" }
};

export const CURRICULUM_STATUS_META: Record<CurriculumLifecycleStatus, EntityStatus> = {
  activo: { level: "ok", label: "Activo" },
  inactivo: { level: "danger", label: "Inactivo" },
  borrador: { level: "muted", label: "Borrador" }
};

export function getChatbotStatus(bot: Chatbot): EntityStatus {
  return CHATBOT_STATUS_META[bot.status];
}

export function getCurriculumStatus(curriculum: Curriculum): EntityStatus {
  return CURRICULUM_STATUS_META[curriculum.status];
}

export function getCurriculumProgressPct(curriculum: Curriculum): number {
  return curriculum.modules.length
    ? Math.round((curriculum.myProgress.completedModuleIds.length / curriculum.modules.length) * 100)
    : 0;
}
