import type { Chatbot, ChatbotLifecycleStatus, ChatbotMap, ChatbotSettings, TeamMember } from "./Chatbot";

export interface CreateChatbotInput {
  name: string;
  dept: string;
  description: string;
  status: ChatbotLifecycleStatus;
  settings: ChatbotSettings;
  iconUrl?: string;
}

export interface IChatbotsRepository {
  FindAll(): Promise<ChatbotMap>;
  Create(input: CreateChatbotInput): Promise<Chatbot>;
  Archive(id: string): Promise<void>;
  UpdateSettings(id: string, patch: Partial<ChatbotSettings>): Promise<void>;
  UpdateStatus(id: string, status: ChatbotLifecycleStatus): Promise<void>;
  UpdateIcon(id: string, iconUrl: string | undefined): Promise<void>;
  AddDocument(id: string, file: File, name: string, origin: string): Promise<void>;
  RemoveDocument(id: string, index: number): Promise<void>;
  AddTeamMember(id: string, member: TeamMember): Promise<void>;
  RemoveTeamMember(id: string, index: number): Promise<void>;
}
