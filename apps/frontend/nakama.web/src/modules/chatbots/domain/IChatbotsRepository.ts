import type { Chatbot, ChatbotLifecycleStatus, ChatbotMap, ChatbotSettings, TeamMember } from "./Chatbot";

export interface CreateChatbotInput {
  name: string;
  dept: string;
  general: boolean;
  description: string;
  status: ChatbotLifecycleStatus;
  settings: ChatbotSettings;
  iconUrl?: string;
}

export interface IChatbotsRepository {
  FindAll(): Promise<ChatbotMap>;
  Create(input: CreateChatbotInput): Promise<Chatbot>;
  Archive(key: string): Promise<void>;
  UpdateSettings(key: string, patch: Partial<ChatbotSettings>): Promise<void>;
  UpdateStatus(key: string, status: ChatbotLifecycleStatus): Promise<void>;
  UpdateIcon(key: string, iconUrl: string | undefined): Promise<void>;
  RemoveDocument(key: string, index: number): Promise<void>;
  AddTeamMember(key: string, member: TeamMember): Promise<void>;
  RemoveTeamMember(key: string, index: number): Promise<void>;
}
