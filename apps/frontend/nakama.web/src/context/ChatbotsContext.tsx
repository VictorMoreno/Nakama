import { createContext, useContext, useMemo, useState, type ReactNode } from "react";
import { mockChatbots } from "../data/mockChatbots";
import { slugify } from "../lib/text";
import type { Chatbot, ChatbotLifecycleStatus, ChatbotMap, ChatbotSettings, TeamMember, TeamRole } from "../types/chatbot";

export interface CreateChatbotInput {
  name: string;
  dept: string;
  general: boolean;
  description: string;
  status: ChatbotLifecycleStatus;
  settings: ChatbotSettings;
  iconUrl?: string;
}

interface ChatbotsContextValue {
  chatbots: ChatbotMap;
  activeChatbots: Chatbot[];
  currentBotKey: string;
  setCurrentBotKey: (key: string) => void;
  isAdmin: boolean;
  setIsAdmin: (admin: boolean) => void;

  createChatbot: (input: CreateChatbotInput) => string;
  archiveChatbot: (key: string) => void;
  updateSettings: (key: string, patch: Partial<ChatbotSettings>) => void;
  updateStatus: (key: string, status: ChatbotLifecycleStatus) => void;
  updateIcon: (key: string, iconUrl: string | undefined) => void;
  removeDocument: (key: string, index: number) => void;
  addTeamMember: (key: string, member: TeamMember) => void;
  removeTeamMember: (key: string, index: number) => void;
}

const ChatbotsContext = createContext<ChatbotsContextValue | null>(null);

const OWNER_ROLES: TeamRole[] = ["Propietario", "Propietaria"];

export function ChatbotsProvider({ children }: { children: ReactNode }) {
  const [chatbots, setChatbots] = useState<ChatbotMap>(mockChatbots);
  const [currentBotKey, setCurrentBotKey] = useState<string>("ecu");
  const [isAdmin, setIsAdmin] = useState(true);

  const activeChatbots = useMemo(
    () => Object.values(chatbots).filter((bot) => !bot.archived),
    [chatbots]
  );

  function updateBot(key: string, updater: (bot: Chatbot) => Chatbot) {
    setChatbots((prev) => {
      const bot = prev[key];
      if (!bot) return prev;
      return { ...prev, [key]: updater(bot) };
    });
  }

  function createChatbot(input: CreateChatbotInput): string {
    const { name, dept, general, description, status, settings, iconUrl } = input;
    const key = slugify(name, Object.keys(chatbots));
    const trimmedDescription = description.trim();
    const fallbackDescription = general
      ? "Chatbot general de empresa. Todavía sin documentos."
      : dept
        ? `Chatbot de equipo para ${dept}. Todavía sin documentos.`
        : "Chatbot recién creado. Todavía sin documentos.";
    const newBot: Chatbot = {
      key,
      label: name,
      description: trimmedDescription || fallbackDescription,
      status,
      lastUsed: "Aún no has usado este chatbot",
      context:
        "Chatbot recién creado" +
        (general ? " para toda la empresa" : dept ? ` para el equipo de <strong>${dept}</strong>` : "") +
        ". Todavía no tiene documentos — añade contenido en la pestaña Documentos para que pueda empezar a responder.",
      opening: null,
      chips: [],
      settings,
      general,
      iconUrl,
      docs: [],
      gaps: [],
      topQuestions: [],
      team: [{ name: "Iker Sola", role: "Propietario" }],
      stats: [
        { label: "Tiempo hasta productividad", value: "—", unit: "sin datos todavía", bar: 0 },
        { label: "Preguntas directas a seniors / semana", value: "—", unit: "sin datos todavía", bar: 0 },
        { label: "Respuestas útiles del bot", value: "—", unit: "sin datos todavía", bar: 0 },
        { label: "Documentos ingestados", value: "0", unit: "objetivo >100", bar: 0 }
      ]
    };
    setChatbots((prev) => ({ ...prev, [key]: newBot }));
    return key;
  }

  function archiveChatbot(key: string) {
    updateBot(key, (bot) => ({ ...bot, archived: true }));
  }

  function updateSettings(key: string, patch: Partial<ChatbotSettings>) {
    updateBot(key, (bot) => ({ ...bot, settings: { ...bot.settings, ...patch } }));
  }

  function updateStatus(key: string, status: ChatbotLifecycleStatus) {
    updateBot(key, (bot) => ({ ...bot, status }));
  }

  function updateIcon(key: string, iconUrl: string | undefined) {
    updateBot(key, (bot) => ({ ...bot, iconUrl }));
  }

  function removeDocument(key: string, index: number) {
    updateBot(key, (bot) => ({ ...bot, docs: bot.docs.filter((_, i) => i !== index) }));
  }

  function addTeamMember(key: string, member: TeamMember) {
    updateBot(key, (bot) => ({ ...bot, team: [...bot.team, member] }));
  }

  function removeTeamMember(key: string, index: number) {
    updateBot(key, (bot) => {
      const target = bot.team[index];
      if (target && OWNER_ROLES.includes(target.role)) return bot; // never remove the owner
      return { ...bot, team: bot.team.filter((_, i) => i !== index) };
    });
  }

  const value: ChatbotsContextValue = {
    chatbots,
    activeChatbots,
    currentBotKey,
    setCurrentBotKey,
    isAdmin,
    setIsAdmin,
    createChatbot,
    archiveChatbot,
    updateSettings,
    updateStatus,
    updateIcon,
    removeDocument,
    addTeamMember,
    removeTeamMember
  };

  return <ChatbotsContext.Provider value={value}>{children}</ChatbotsContext.Provider>;
}

export function useChatbots(): ChatbotsContextValue {
  const ctx = useContext(ChatbotsContext);
  if (!ctx) throw new Error("useChatbots must be used within a ChatbotsProvider");
  return ctx;
}

export function isOwnerRole(role: TeamRole): boolean {
  return OWNER_ROLES.includes(role);
}
