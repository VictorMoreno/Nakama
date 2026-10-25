import type {
  AddFormationModuleRequest,
  AddFormationTeamMemberRequest,
  CreateFormationRequest,
  FormationDetailResponse,
  FormationDocumentResponse,
  FormationMemberRow,
  FormationModuleResponse,
  FormationSummaryResponse,
  GenerateQuizQuestionsRequest,
  GeneratedNakamaClient,
  QuizQuestionResponse,
  SubmitFormationExamRequest,
  UpdateFormationDetailsRequest,
  UpdateFormationModuleRequest,
  UpdateFormationStatusRequest
} from "../../../shared/infrastructure/GeneratedNakamaClient";
import type { DocumentFile } from "../../../shared/domain/DocumentFile";
import { dataUrlToFileParameter } from "../../../shared/infrastructure/dataUrlToFileParameter";
import { formatDate } from "../../../shared/infrastructure/formatDate";
import type { Page, PageQuery } from "../../../shared/domain/Page";
import type {
  ExamResult,
  Formation,
  FormationLifecycleStatus,
  FormationDocument,
  FormationListItem,
  FormationMap,
  FormationMember,
  FormationModule,
  ModuleType,
  QuizQuestion
} from "../domain/Formation";
import type { LlmProviderKind } from "../../llmProviders/domain/LlmProviderConfig";
import type { CreateFormationInput, GenerateQuizInput, IFormationsRepository } from "../domain/IFormationsRepository";

// Write side: backend enums serialize as their ordinal (0-indexed, declaration order).
// Read side: response DTOs already stringify enums via `.ToString()` (PascalCase member
// name, e.g. "Active") - the two directions are asymmetric, never share one lookup table.
const STATUS_TO_ORDINAL: Record<FormationLifecycleStatus, number> = { borrador: 0, activo: 1, inactivo: 2 };
const STATUS_FROM_API: Record<string, FormationLifecycleStatus> = { Draft: "borrador", Active: "activo", Inactive: "inactivo" };
const STATUS_TO_API: Record<FormationLifecycleStatus, string> = { borrador: "Draft", activo: "Active", inactivo: "Inactive" };

const MODULE_TYPE_TO_ORDINAL: Record<ModuleType, number> = { video: 0, documento: 1, ejercicio: 2, examen: 3 };
const MODULE_TYPE_FROM_API: Record<string, ModuleType> = { Video: "video", Document: "documento", Exercise: "ejercicio", Exam: "examen" };

function toQuizQuestion(row: QuizQuestionResponse): QuizQuestion {
  return { question: row.question ?? "", options: row.options ?? [], correctIndex: row.correctIndex };
}

function toFormationModule(row: FormationModuleResponse): FormationModule {
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

function toFormationDocument(row: FormationDocumentResponse): FormationDocument {
  return { id: row.id ?? "", name: row.name ?? "", origin: row.origin ?? "", updated: formatDate(row.updated) };
}

function toFormationMember(row: FormationMemberRow): FormationMember {
  return { userId: row.userId ?? "", progress: row.progress ?? 0 };
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
    modules: (detail.modules ?? []).map(toFormationModule),
    docs: (detail.docs ?? []).map(toFormationDocument),
    team: (detail.team ?? []).map(toFormationMember),
    archived: summary.archived ?? false,
    iconUrl: summary.iconUrl,
    isEnrolled: summary.isEnrolled ?? false,
    progressPct: summary.progressPct ?? 0,
    completedModulesCount: summary.completedModulesCount ?? 0
  };
}

function toFormationListItem(summary: FormationSummaryResponse): FormationListItem {
  return {
    id: summary.id ?? "",
    title: summary.title ?? "",
    audience: summary.audience ?? "",
    description: summary.description ?? "",
    status: STATUS_FROM_API[summary.status ?? "Draft"] ?? "borrador",
    iconUrl: summary.iconUrl,
    isEnrolled: summary.isEnrolled ?? false,
    progressPct: summary.progressPct ?? 0
  };
}

const PROVIDER_TO_ORDINAL: Record<LlmProviderKind, number> = { anthropic: 0, openai: 1, local: 2 };

export class ApiFormationsRepository implements IFormationsRepository {
  private readonly client: GeneratedNakamaClient;

  constructor(client: GeneratedNakamaClient) {
    this.client = client;
  }

  async FindAll(): Promise<FormationMap> {
    const summaries = await this.client.formationsAll();
    const entries = await Promise.all(
      summaries.map(async (summary) => {
        const id = summary.id ?? "";
        const detail = await this.client.formationsGET(id);
        return [id, toFormation(summary, detail)] as const;
      })
    );
    return Object.fromEntries(entries);
  }

  async FindPage(query: PageQuery<FormationLifecycleStatus>): Promise<Page<FormationListItem>> {
    const response = await this.client.paged2(
      query.page,
      query.pageSize,
      query.search || undefined,
      query.status ? STATUS_TO_API[query.status] : undefined,
      query.archived,
      query.progress
    );
    return {
      items: (response.items ?? []).map(toFormationListItem),
      totalCount: response.totalCount ?? 0,
      totalPages: response.totalPages ?? 0,
      page: response.page ?? query.page,
      pageSize: response.pageSize ?? query.pageSize
    };
  }

  async Create(input: CreateFormationInput): Promise<Formation> {
    const { title, audience, description, status, iconUrl } = input;
    const id = crypto.randomUUID();
    const trimmedDescription = description.trim();

    await this.client.formationsPUT(id, {
      title,
      audience,
      description: trimmedDescription,
      status: STATUS_TO_ORDINAL[status]
    } satisfies CreateFormationRequest);

    if (iconUrl) {
      await this.client.iconPOST2(id, await dataUrlToFileParameter(iconUrl, "icon"));
    }

    // Built locally instead of a follow-up GET, same reasoning as ApiChatbotsRepository.Create.
    return {
      id,
      title,
      audience,
      description: trimmedDescription,
      status,
      iconUrl,
      modules: [],
      docs: [],
      team: [],
      isEnrolled: false,
      progressPct: 0,
      completedModulesCount: 0
    };
  }

  async Archive(id: string): Promise<void> {
    await this.client.archive2(id);
  }

  async Unarchive(id: string): Promise<void> {
    await this.client.unarchive2(id);
  }

  async UpdateStatus(id: string, status: FormationLifecycleStatus): Promise<void> {
    await this.client.status2(id, { status: STATUS_TO_ORDINAL[status] } satisfies UpdateFormationStatusRequest);
  }

  async UpdateDetails(id: string, title: string, audience: string, description: string): Promise<void> {
    await this.client.details2(id, { title, audience, description } satisfies UpdateFormationDetailsRequest);
  }

  async UpdateIcon(id: string, iconUrl: string | undefined): Promise<void> {
    if (iconUrl) {
      await this.client.iconPOST2(id, await dataUrlToFileParameter(iconUrl, "icon"));
    } else {
      await this.client.iconDELETE2(id);
    }
  }

  async AddDocument(id: string, file: File, name: string, origin: string): Promise<void> {
    await this.client.documentsPUT2(id, crypto.randomUUID(), { data: file, fileName: file.name }, name, origin);
  }

  async RemoveDocument(id: string, documentId: string): Promise<void> {
    await this.client.documentsDELETE2(id, documentId);
  }

  async DownloadDocument(id: string, documentId: string): Promise<DocumentFile> {
    const response = await this.client.file2(id, documentId);
    return { blob: response.data, fileName: response.fileName ?? "documento" };
  }

  async GenerateQuizQuestions(input: GenerateQuizInput): Promise<QuizQuestion[]> {
    const response = await this.client.quizQuestions(input.formationId, {
      documentIds: input.documentIds,
      provider: PROVIDER_TO_ORDINAL[input.provider],
      model: input.model,
      questionCount: input.questionCount,
      topic: input.topic
    } satisfies GenerateQuizQuestionsRequest);

    return (response.questions ?? []).map((row) => ({
      question: row.question ?? "",
      options: row.options ?? [],
      correctIndex: row.correctIndex ?? 0
    }));
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

  async CompleteModule(id: string, moduleId: string): Promise<void> {
    await this.client.complete(id, moduleId);
  }

  async SubmitExam(id: string, moduleId: string, answers: number[]): Promise<ExamResult> {
    await this.client.exam(id, moduleId, { answers } satisfies SubmitFormationExamRequest);
    // The submit endpoint answers 204 (it is a command); the server-side grade is read back from the progress.
    const progress = await this.client.enrollment(id);
    const attempts = (progress.examAttempts ?? []).filter((attempt) => attempt.moduleId === moduleId);
    const last = attempts[attempts.length - 1];
    if (!last) throw new Error("The exam attempt was not recorded.");
    return { score: last.score ?? 0, passed: last.passed ?? false };
  }

  async AddTeamMember(id: string, userId: string): Promise<void> {
    await this.client.teamPOST2(id, { userId } satisfies AddFormationTeamMemberRequest);
  }

  async RemoveTeamMember(id: string, userId: string): Promise<void> {
    await this.client.teamDELETE2(id, userId);
  }
}
