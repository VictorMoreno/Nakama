export type LlmProvider = "anthropic" | "openai" | "local";

export type ChatbotLifecycleStatus = "activo" | "inactivo" | "borrador";

export interface ChatbotSettings {
  socratic: boolean;
  explicaMas: boolean;
  provider: LlmProvider;
  model: string;
}

export type DocStatus = "ok" | "pending";

export interface ChatbotDocument {
  id: string;
  name: string;
  origin: string;
  status: DocStatus;
  statusLabel: string;
  updated: string;
}

export interface DocumentFile {
  blob: Blob;
  fileName: string;
}

export type TeamRole = "Propietario" | "Propietaria" | "Editor" | "Lector";

export interface TeamMember {
  userId: string;
  role: TeamRole;
}

const OWNER_ROLES: TeamRole[] = ["Propietario", "Propietaria"];

export function isOwnerRole(role: TeamRole): boolean {
  return OWNER_ROLES.includes(role);
}

export interface ChatExample {
  q: string;
  a: string;
  attachments?: string[];
  useful?: boolean | null;
}

export interface ConversationSummary {
  id: string;
  botKey: string;
  title: string;
  dateLabel: string;
  exchanges: ChatExample[];
}

export interface Chatbot {
  id: string;
  label: string;
  description: string;
  status: ChatbotLifecycleStatus;
  context: string;
  settings: ChatbotSettings;
  docs: ChatbotDocument[];
  team: TeamMember[];
  archived?: boolean;
  iconUrl?: string;
}

export type ChatbotMap = Record<string, Chatbot>;

/** Fila de las páginas de listado: solo el resumen, sin cargar el detalle de cada chatbot. */
export interface ChatbotListItem {
  id: string;
  label: string;
  description: string;
  status: ChatbotLifecycleStatus;
  docsCount: number;
  teamCount: number;
  iconUrl?: string;
}
