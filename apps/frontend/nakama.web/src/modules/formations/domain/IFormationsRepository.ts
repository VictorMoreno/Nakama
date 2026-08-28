import type { Formation, FormationLifecycleStatus, FormationMap, FormationModule } from "./Formation";

export interface CreateFormationInput {
  title: string;
  audience: string;
  description: string;
  status: FormationLifecycleStatus;
  linkedChatbotKey?: string;
  iconUrl?: string;
}

export interface IFormationsRepository {
  FindAll(): Promise<FormationMap>;
  Create(input: CreateFormationInput): Promise<Formation>;
  Archive(key: string): Promise<void>;
  UpdateStatus(key: string, status: FormationLifecycleStatus): Promise<void>;
  UpdateIcon(key: string, iconUrl: string | undefined): Promise<void>;
  AddModule(key: string, module: Omit<FormationModule, "id">): Promise<void>;
  RemoveModule(key: string, moduleId: string): Promise<void>;
  ToggleModuleGating(key: string, moduleId: string): Promise<void>;
  CompleteModule(key: string, moduleId: string, score?: number): Promise<void>;
}
