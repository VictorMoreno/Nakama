/* eslint-disable react-refresh/only-export-components */
import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from "react";
import type { Chatbot, ChatbotMap, ChatbotSettings, TeamMember } from "../../../modules/chatbots/domain/Chatbot";
import type { CreateChatbotInput, IChatbotsRepository } from "../../../modules/chatbots/domain/IChatbotsRepository";
import { Services } from "../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../shared/infrastructure/injection/useInjection";
import { useUser } from "./UserContext";

export type { CreateChatbotInput };

interface ChatbotsContextValue {
  chatbots: ChatbotMap;
  activeChatbots: Chatbot[];
  isLoading: boolean;
  currentBotKey: string;
  setCurrentBotKey: (key: string) => void;

  createChatbot: (input: CreateChatbotInput) => Promise<string>;
  archiveChatbot: (key: string) => Promise<void>;
  unarchiveChatbot: (key: string) => Promise<void>;
  updateSettings: (key: string, patch: Partial<ChatbotSettings>) => Promise<void>;
  updateStatus: (key: string, status: Chatbot["status"]) => Promise<void>;
  updateDetails: (key: string, label: string, description: string) => Promise<void>;
  updateIcon: (key: string, iconUrl: string | undefined) => Promise<void>;
  addDocument: (key: string, file: File, name: string, origin: string) => Promise<void>;
  removeDocument: (key: string, documentId: string) => Promise<void>;
  addTeamMember: (key: string, member: TeamMember) => Promise<void>;
  removeTeamMember: (key: string, userId: string) => Promise<void>;
}

const ChatbotsContext = createContext<ChatbotsContextValue | null>(null);
const NO_CHATBOTS: ChatbotMap = {};

export function ChatbotsProvider({ children }: { children: ReactNode }) {
  const repo = useInjection<IChatbotsRepository>(Services.ChatbotsRepository.id);

  const [loadedChatbots, setChatbots] = useState<ChatbotMap>(NO_CHATBOTS);
  const [loadedFor, setLoadedFor] = useState<string | null>(null);
  const [currentBotKey, setCurrentBotKey] = useState<string>("");

  const { currentUser } = useUser();
  const userId: string | null = currentUser?.id ?? null;
  const chatbots: ChatbotMap = userId && loadedFor === userId ? loadedChatbots : NO_CHATBOTS;
  const isLoading: boolean = userId !== null && loadedFor !== userId;

  useEffect(() => {
    if (!userId) return;

    let cancelled = false;
    repo.FindAll().then((data) => {
      if (!cancelled) {
        setChatbots(data);
        setLoadedFor(userId);
      }
    });
    return () => {
      cancelled = true;
    };
  }, [repo, userId]);

  const activeChatbots = useMemo(
    () =>
      Object.values(chatbots)
        .filter((bot) => !bot.archived)
        .sort((a, b) => a.label.localeCompare(b.label)),
    [chatbots]
  );

  useEffect(() => {
    if (activeChatbots.length === 0) return;
    if (!activeChatbots.some((bot) => bot.id === currentBotKey)) {
      setCurrentBotKey(activeChatbots[0].id);
    }
  }, [activeChatbots, currentBotKey]);

  async function createChatbot(input: CreateChatbotInput): Promise<string> {
    const bot = await repo.Create(input);
    setChatbots((prev) => ({ ...prev, [bot.id]: bot }));
    return bot.id;
  }

  async function archiveChatbot(key: string) {
    await repo.Archive(key);
    setChatbots((prev) => (prev[key] ? { ...prev, [key]: { ...prev[key], archived: true } } : prev));
  }

  async function unarchiveChatbot(key: string) {
    await repo.Unarchive(key);
    setChatbots((prev) => (prev[key] ? { ...prev, [key]: { ...prev[key], archived: false } } : prev));
  }

  async function updateSettings(key: string, patch: Partial<ChatbotSettings>) {
    await repo.UpdateSettings(key, patch);
    setChatbots((prev) =>
      prev[key]
        ? {
            ...prev,
            [key]: { ...prev[key], settings: { ...prev[key].settings, ...patch } }
          }
        : prev
    );
  }

  async function updateStatus(key: string, status: Chatbot["status"]) {
    await repo.UpdateStatus(key, status);
    setChatbots((prev) => (prev[key] ? { ...prev, [key]: { ...prev[key], status } } : prev));
  }

  async function updateDetails(key: string, label: string, description: string) {
    await repo.UpdateDetails(key, label, description);
    setChatbots((prev) => (prev[key] ? { ...prev, [key]: { ...prev[key], label, description } } : prev));
  }

  async function updateIcon(key: string, iconUrl: string | undefined) {
    await repo.UpdateIcon(key, iconUrl);
    setChatbots((prev) => (prev[key] ? { ...prev, [key]: { ...prev[key], iconUrl } } : prev));
  }

  async function addDocument(key: string, file: File, name: string, origin: string) {
    await repo.AddDocument(key, file, name, origin);
    const all = await repo.FindAll();
    setChatbots(all);
  }

  async function reloadChatbots() {
    try {
      setChatbots(await repo.FindAll());
    } catch {
      return;
    }
  }

  async function removeDocument(key: string, documentId: string) {
    try {
      await repo.RemoveDocument(key, documentId);
    } finally {
      await reloadChatbots();
    }
  }

  async function addTeamMember(key: string, member: TeamMember) {
    await repo.AddTeamMember(key, member);
    setChatbots((prev) => (prev[key] ? { ...prev, [key]: { ...prev[key], team: [...prev[key].team, member] } } : prev));
  }

  async function removeTeamMember(key: string, userId: string) {
    try {
      await repo.RemoveTeamMember(key, userId);
    } finally {
      await reloadChatbots();
    }
  }

  const value: ChatbotsContextValue = {
    chatbots,
    activeChatbots,
    isLoading,
    currentBotKey,
    setCurrentBotKey,
    createChatbot,
    archiveChatbot,
    unarchiveChatbot,
    updateSettings,
    updateStatus,
    updateDetails,
    updateIcon,
    addDocument,
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
