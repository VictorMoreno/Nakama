import type { Page, PageQuery } from "../../../shared/domain/Page";
import type { DocumentFile } from "../../../shared/domain/DocumentFile";
import type { LlmProviderKind } from "../../llmProviders/domain/LlmProviderConfig";
import type {
  ExamResult,
  Formation,
  FormationLifecycleStatus,
  FormationListItem,
  FormationMap,
  FormationModule,
  QuizQuestion
} from "./Formation";

export interface CreateFormationInput {
  title: string;
  audience: string;
  description: string;
  status: FormationLifecycleStatus;
  iconUrl?: string;
}

export interface GenerateQuizInput {
  formationId: string;
  documentIds: string[];
  provider: LlmProviderKind;
  model: string;
  questionCount: number;
  topic: string;
}

export interface IFormationsRepository {
  FindAll(): Promise<FormationMap>;
  FindPage(query: PageQuery<FormationLifecycleStatus>): Promise<Page<FormationListItem>>;
  Create(input: CreateFormationInput): Promise<Formation>;
  Archive(id: string): Promise<void>;
  Unarchive(id: string): Promise<void>;
  UpdateStatus(id: string, status: FormationLifecycleStatus): Promise<void>;
  UpdateDetails(id: string, title: string, audience: string, description: string): Promise<void>;
  UpdateIcon(id: string, iconUrl: string | undefined): Promise<void>;
  AddModule(id: string, module: Omit<FormationModule, "id">): Promise<void>;
  UpdateModule(id: string, module: FormationModule): Promise<void>;
  RemoveModule(id: string, moduleId: string): Promise<void>;
  AddDocument(id: string, file: File, name: string, origin: string): Promise<void>;
  RemoveDocument(id: string, documentId: string): Promise<void>;
  DownloadDocument(id: string, documentId: string): Promise<DocumentFile>;
  GenerateQuizQuestions(input: GenerateQuizInput): Promise<QuizQuestion[]>;
  ToggleModuleGating(id: string, moduleId: string): Promise<void>;
  CompleteModule(id: string, moduleId: string): Promise<void>;
  SubmitExam(id: string, moduleId: string, answers: number[]): Promise<ExamResult>;
  AddTeamMember(id: string, userId: string): Promise<void>;
  RemoveTeamMember(id: string, userId: string): Promise<void>;
}
