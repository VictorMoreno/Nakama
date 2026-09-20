import type {
  AddFormationModuleRequest,
  CompleteFormationModuleRequest,
  CreateFormationRequest,
  FormationDetailResponse,
  FormationMemberRow,
  FormationModuleRow,
  FormationSummaryResponse,
  GeneratedNakamaClient,
  QuizQuestionRow,
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
  return { name: row.name ?? "", progress: row.progress ?? 0 };
}

/**
 * Merges the list + detail endpoints into the flat `Formation` shape the rest of the app
 * already expects (N+1 fetch per FindAll - same accepted tradeoff as ApiChatbotsRepository).
 */
function toFormation(summary: FormationSummaryResponse, detail: FormationDetailResponse): Formation {
  return {
    id: summary.id,
    title: summary.title,
    audience: summary.audience,
    description: summary.description,
    status: STATUS_FROM_API[summary.status] ?? "borrador",
    linkedChatbotKey: detail.linkedChatbotKey,
    modules: detail.modules.map(toFormationModule),
    myProgress: { completedModuleIds: detail.completedModuleIds, examScores: detail.examScores },
    team: detail.team.map(toFormationMember),
    archived: summary.archived,
    iconUrl: summary.iconUrl
  };
}

export class ApiFormationsRepository implements IFormationsRepository {
  private readonly client: GeneratedNakamaClient;

  constructor(client: GeneratedNakamaClient) {
    this.client = client;
  }

  async FindAll(): Promise<FormationMap> {
    const summaries = await this.client.formationsAll();
    const entries = await Promise.all(
      summaries.map(async (summary) => {
        const detail = await this.client.formationsGET(summary.id);
        return [summary.id, toFormation(summary, detail)] as const;
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
      myProgress: { completedModuleIds: [], examScores: {} },
      team: []
    };
  }

  async Archive(): Promise<void> {
    // No backend endpoint for this yet (not among the 7 formation routes built this
    // session) - failing loudly rather than pretending it saved.
    throw new Error("ApiFormationsRepository.Archive: no existe todavía un endpoint en la API para archivar una formación.");
  }

  async UpdateStatus(id: string, status: FormationLifecycleStatus): Promise<void> {
    await this.client.status2(id, { status: STATUS_TO_ORDINAL[status] } satisfies UpdateFormationStatusRequest);
  }

  async UpdateIcon(): Promise<void> {
    throw new Error("ApiFormationsRepository.UpdateIcon: no existe todavía un endpoint en la API para actualizar el icono de una formación.");
  }

  async AddModule(id: string, module: Omit<FormationModule, "id">): Promise<void> {
    const moduleId = crypto.randomUUID();
    await this.client.modules(id, moduleId, {
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

  async RemoveModule(): Promise<void> {
    // No DELETE-module endpoint exists on the backend - same "fail loudly" rule as Archive/
    // UpdateIcon above.
    throw new Error("ApiFormationsRepository.RemoveModule: no existe todavía un endpoint en la API para eliminar un módulo.");
  }

  async ToggleModuleGating(id: string, moduleId: string): Promise<void> {
    await this.client.toggleGating(id, moduleId);
  }

  async CompleteModule(id: string, moduleId: string, score?: number): Promise<void> {
    await this.client.complete(id, moduleId, { score } satisfies CompleteFormationModuleRequest);
  }
}
