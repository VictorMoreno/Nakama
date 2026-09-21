export type LlmProvider = "anthropic" | "openai" | "local";

/** Estado configurado por el admin del chatbot — determina el LED de la card y el status-tag de la ficha */
export type ChatbotLifecycleStatus = "activo" | "inactivo" | "borrador";

export interface ChatbotSettings {
  socratic: boolean;
  explicaMas: boolean;
  provider: LlmProvider;
}

export type DocStatus = "ok" | "pending";

export interface ChatbotDocument {
  id?: string;
  name: string;
  origin: string;
  status: DocStatus;
  statusLabel: string;
  updated: string;
  url?: string;
}

export interface GapQuestion {
  q: string;
  count: number;
}

export type TeamRole = "Propietario" | "Propietaria" | "Editor" | "Lector";

export interface TeamMember {
  name: string;
  role: TeamRole;
}

const OWNER_ROLES: TeamRole[] = ["Propietario", "Propietaria"];

/** Domain invariant: the owner of a chatbot's team can never be removed. */
export function isOwnerRole(role: TeamRole): boolean {
  return OWNER_ROLES.includes(role);
}

export interface StatTile {
  label: string;
  value: string;
  unit: string;
  bar: number;
}

/** A scripted example exchange used to demo the RAG answer shape (direct answer, citation, deeper explanation, Socratic variant). */
export interface ChatExample {
  q: string;
  a: string;
  cite: string;
  more: string;
  socratic: string;
}

/** A past conversation the user can reopen from the chat history panel, grouped by the chatbot it belongs to. */
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
  /** Frase corta mostrada en el listado (distinta de `context`, que es HTML largo para el panel del chat) */
  description: string;
  /** Configurado por el admin del chatbot */
  status: ChatbotLifecycleStatus;
  lastUsed: string;
  context: string;
  /** null for a freshly created chatbot with no indexed content yet */
  opening: ChatExample | null;
  chips: ChatExample[];
  settings: ChatbotSettings;
  docs: ChatbotDocument[];
  gaps: GapQuestion[];
  /** Preguntas más repetidas (RF20), independientemente de si la respuesta fue buena */
  topQuestions: GapQuestion[];
  team: TeamMember[];
  stats: StatTile[];
  archived?: boolean;
  /** true para el chatbot transversal de empresa (RF19), nutrido con información general en vez de la de un departamento */
  general?: boolean;
  /** Imagen subida por un admin (al crear o desde la pestaña General), mostrada en la card del listado en vez del icono por defecto */
  iconUrl?: string;
}

export type ChatbotMap = Record<string, Chatbot>;
