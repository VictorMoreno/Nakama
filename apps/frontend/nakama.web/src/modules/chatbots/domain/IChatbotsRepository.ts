import type { DocumentFile } from "../../../shared/domain/DocumentFile";
import type { Page, PageQuery } from "../../../shared/domain/Page";
import type {
  Chatbot,
  ChatbotLifecycleStatus,
  ChatbotListItem,
  ChatbotMap,
  ChatbotSettings,
  TeamMember
} from "./Chatbot";

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
  FindPage(query: PageQuery<ChatbotLifecycleStatus>): Promise<Page<ChatbotListItem>>;
  Create(input: CreateChatbotInput): Promise<Chatbot>;
  Archive(id: string): Promise<void>;
  UpdateSettings(id: string, patch: Partial<ChatbotSettings>): Promise<void>;
  UpdateStatus(id: string, status: ChatbotLifecycleStatus): Promise<void>;
  UpdateIcon(id: string, iconUrl: string | undefined): Promise<void>;
  AddDocument(id: string, file: File, name: string, origin: string): Promise<void>;
  RemoveDocument(id: string, documentId: string): Promise<void>;
  DownloadDocument(id: string, documentId: string): Promise<DocumentFile>;
  AddTeamMember(id: string, member: TeamMember): Promise<void>;
  RemoveTeamMember(id: string, userId: string): Promise<void>;
}
