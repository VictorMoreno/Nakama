import type { Page, PageQuery } from "../../../shared/domain/Page";
import type { Formation, FormationLifecycleStatus, FormationListItem, FormationMap, FormationModule } from "./Formation";

export interface CreateFormationInput {
  title: string;
  audience: string;
  description: string;
  status: FormationLifecycleStatus;
  iconUrl?: string;
}

export interface IFormationsRepository {
  FindAll(): Promise<FormationMap>;
  FindPage(query: PageQuery<FormationLifecycleStatus>): Promise<Page<FormationListItem>>;
  Create(input: CreateFormationInput): Promise<Formation>;
  Archive(id: string): Promise<void>;
  UpdateStatus(id: string, status: FormationLifecycleStatus): Promise<void>;
  UpdateIcon(id: string, iconUrl: string | undefined): Promise<void>;
  AddModule(id: string, module: Omit<FormationModule, "id">): Promise<void>;
  UpdateModule(id: string, module: FormationModule): Promise<void>;
  RemoveModule(id: string, moduleId: string): Promise<void>;
  ToggleModuleGating(id: string, moduleId: string): Promise<void>;
  CompleteModule(id: string, moduleId: string, score?: number): Promise<void>;
  AddTeamMember(id: string, userId: string): Promise<void>;
  RemoveTeamMember(id: string, userId: string): Promise<void>;
}
