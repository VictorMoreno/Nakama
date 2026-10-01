import type {
  AddFormationModuleRequest,
  AddFormationTeamMemberRequest,
  CompleteFormationModuleRequest,
  CreateFormationRequest,
  FormationDetailResponse,
  FormationMemberRow,
  FormationModuleRow,
  FormationSummaryResponse,
  GeneratedNakamaClient,
  QuizQuestionRow,
  UpdateFormationModuleRequest,
  UpdateFormationStatusRequest
} from "../../../shared/infrastructure/GeneratedNakamaClient";
import type { Formation, FormationLifecycleStatus, FormationMap, FormationMember, FormationModule, ModuleType, QuizQuestion } from "../domain/Formation";
import type { CreateFormationInput, IFormationsRepository } from "../domain/IFormationsRepository";

// Write side: backend enums serialize as their ordinal (0-indexed, declaration order).
// Read side: response DTOs already stringify enums via `.ToString()` (PascalCase member
// name, e.g. "Active") - the two directions are asymmetric, never share one lookup table.
const STATUS_TO_ORDINAL: Record<FormationLifecycleStatus, number> = { borrador: 0, activo: 1, inactivo: 2 };
const STATUS_FROM_API: Record<string, FormationLifecycleStatus> = { Draft: "borrador", Active: "activo", Inactive: "inactivo" };

const MODULE_TYPE_TO_ORDINAL: Record<ModuleType, number> = { video: 0, documento: 1, ejercicio: 2, examen: 3 };
const MODULE_TYPE_FROM_API: Record<string, ModuleType> = { Video: "video", Document: "documento", Exercise: "ejercicio", Exam: "examen" };

function toQuizQuestion(row: QuizQuestionRow): QuizQuestion {
  return { question: row.question ?? "", options: row.options ?? [], correctIndex: row.correctIndex ?? 0 };
}

function toFormationModule(row: FormationModuleRow): FormationModule {
  return {
    id: row.id ?? "",
    title: row.title ?? "",
    type: MODULE_TYPE_FROM_API[row.type ?? "Document"] ?? "documento",
    summary: row.summary ?? "",
    gated: row.gated ?? false,
    aiDraft: row.aiDraft ?? false,
    resourceUrl: row.resourceUrl,
    durationMin: row.durationMin,
    quiz: row.quiz?.map(toQuizQuestion),
    passingScore: row.passingScore
  };
}

function toFormationMember(row: FormationMemberRow): FormationMember {
  return { userId: row.userId ?? "", progress: row.progress ?? 0 };
}

// No generated client method exists for these - see the identical helpers in
// ApiChatbotsRepository.ts for the full rationale (multipart endpoints, same base URL
// convention as Container.ts). Kept duplicated rather than shared - two call sites, matches
// this codebase's existing preference for independent repository files over a premature
// shared module.
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? "";

async function uploadIcon(id: string, dataUrl: string): Promise<void> {
  const blob = await (await fetch(dataUrl)).blob();
  const form = new FormData();
  form.append("file", blob, "icon");
  const response = await fetch(`${API_BASE_URL}/api/formations/${id}/icon`, { method: "POST", body: form });
  if (!response.ok) throw new Error(`No se pudo subir el icono (HTTP ${response.status}).`);
}

async function deleteIcon(id: string): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/api/formations/${id}/icon`, { method: "DELETE" });
  if (!response.ok) throw new Error(`No se pudo quitar el icono (HTTP ${response.status}).`);
}

async function archiveFormation(id: string): Promise<void> {
  const response = await fetch(`${API_BASE_URL}/api/formations/${id}/archive`, { method: "POST" });
  if (!response.ok) throw new Error(`No se pudo eliminar la formación (HTTP ${response.status}).`);
}

/**
 * Merges the list + detail endpoints into the flat `Formation` shape the rest of the app
 * already expects (N+1 fetch per FindAll - same accepted tradeoff as ApiChatbotsRepository).
 */
function toFormation(summary: FormationSummaryResponse, detail: FormationDetailResponse): Formation {
  return {
    id: summary.id ?? "",
    title: summary.title ?? "",
    audience: summary.audience ?? "",
    description: summary.description ?? "",
    status: STATUS_FROM_API[summary.status ?? "Draft"] ?? "borrador",
    linkedChatbotKey: detail.linkedChatbotKey,
    modules: (detail.modules ?? []).map(toFormationModule),
    team: (detail.team ?? []).map(toFormationMember),
    archived: summary.archived ?? false,
    iconUrl: summary.iconUrl,
    isEnrolled: summary.isEnrolled ?? false,
    progressPct: summary.progressPct ?? 0,
    completedModulesCount: summary.completedModulesCount ?? 0
  };
}

export class ApiFormationsRepository implements IFormationsRepository {
  private readonly client: GeneratedNakamaClient;

  constructor(client: GeneratedNakamaClient) {
    this.client = client;
  }

  async FindAll(userId: string, isAdmin: boolean): Promise<FormationMap> {
    const summaries = await this.client.formationsAll(userId, isAdmin);
    const entries = await Promise.all(
      summaries.map(async (summary) => {
        const id = summary.id ?? "";
        const detail = await this.client.formationsGET(id);
        return [id, toFormation(summary, detail)] as const;
      })
    );
    return Object.fromEntries(entries);
  }

  async Create(input: CreateFormationInput): Promise<Formation> {
    const { title, audience, description, status, linkedChatbotKey, iconUrl } = input;
    const id = crypto.randomUUID();
    const trimmedDescription = description.trim();
    const finalAudience = audience || "Sin definir";
    const finalDescription = trimmedDescription || "Formación recién creada. Añade módulos para que el equipo pueda empezar a formarse.";

    await this.client.formationsPUT(id, {
      title,
      audience: finalAudience,
      description: finalDescription,
      status: STATUS_TO_ORDINAL[status],
      linkedChatbotKey
    } satisfies CreateFormationRequest);

    if (iconUrl) {
      await uploadIcon(id, iconUrl);
    }

    // Built locally instead of a follow-up GET, same reasoning as ApiChatbotsRepository.Create.
    return {
      id,
      title,
      audience: finalAudience,
      description: finalDescription,
      status,
      linkedChatbotKey,
      iconUrl,
      modules: [],
      team: [],
      isEnrolled: false,
      progressPct: 0,
      completedModulesCount: 0
    };
  }

  async Archive(id: string): Promise<void> {
    await archiveFormation(id);
  }

  async UpdateStatus(id: string, status: FormationLifecycleStatus): Promise<void> {
    await this.client.status2(id, { status: STATUS_TO_ORDINAL[status] } satisfies UpdateFormationStatusRequest);
  }

  async UpdateIcon(id: string, iconUrl: string | undefined): Promise<void> {
    if (iconUrl) {
      await uploadIcon(id, iconUrl);
    } else {
      await deleteIcon(id);
    }
  }

  async AddModule(id: string, module: Omit<FormationModule, "id">): Promise<void> {
    const moduleId = crypto.randomUUID();
    await this.client.modulesPUT(id, moduleId, {
      title: module.title,
      type: MODULE_TYPE_TO_ORDINAL[module.type],
      summary: module.summary,
      gated: module.gated,
      aiDraft: module.aiDraft,
      resourceUrl: module.resourceUrl,
      durationMin: module.durationMin,
      quiz: module.quiz,
      passingScore: module.passingScore
    } satisfies AddFormationModuleRequest);
  }

  async UpdateModule(id: string, module: FormationModule): Promise<void> {
    await this.client.modulesPATCH(id, module.id, {
      title: module.title,
      type: MODULE_TYPE_TO_ORDINAL[module.type],
      summary: module.summary,
      gated: module.gated,
      aiDraft: module.aiDraft,
      resourceUrl: module.resourceUrl,
      durationMin: module.durationMin,
      quiz: module.quiz,
      passingScore: module.passingScore
    } satisfies UpdateFormationModuleRequest);
  }

  async RemoveModule(id: string, moduleId: string): Promise<void> {
    await this.client.modulesDELETE(id, moduleId);
  }

  async ToggleModuleGating(id: string, moduleId: string): Promise<void> {
    await this.client.toggleGating(id, moduleId);
  }

  async CompleteModule(id: string, moduleId: string, userId: string, score?: number): Promise<void> {
    await this.client.complete(id, moduleId, { userId, score } satisfies CompleteFormationModuleRequest);
  }

  async AddTeamMember(id: string, userId: string): Promise<void> {
    await this.client.teamPOST2(id, { userId } satisfies AddFormationTeamMemberRequest);
  }

  async RemoveTeamMember(id: string, userId: string): Promise<void> {
    await this.client.teamDELETE2(id, userId);
  }
}
