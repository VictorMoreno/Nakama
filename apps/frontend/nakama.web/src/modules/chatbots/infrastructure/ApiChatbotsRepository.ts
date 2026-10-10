import type {
  ChatbotDetailResponse,
  ChatbotDocumentResponse,
  ChatbotSummaryResponse,
  ChatPageResponse,
  CreateChatbotRequest,
  GapQuestionRow,
  GeneratedNakamaClient,
  StatTileRow,
  TeamMemberRow,
  UpdateChatbotSettingsRequest,
  UpdateChatbotStatusRequest
} from "../../../shared/infrastructure/GeneratedNakamaClient";
import type {
  Chatbot,
  ChatbotDocument,
  ChatbotLifecycleStatus,
  ChatbotMap,
  ChatbotSettings,
  ChatExample,
  DocStatus,
  GapQuestion,
  LlmProvider,
  StatTile,
  TeamMember,
  TeamRole
} from "../domain/Chatbot";
import type { CreateChatbotInput, IChatbotsRepository } from "../domain/IChatbotsRepository";
import { dataUrlToFileParameter } from "../../../shared/infrastructure/dataUrlToFileParameter";

// Write side: backend enums serialize as their ordinal (0-indexed, declaration order).
// Read side: response DTOs already stringify enums via `.ToString()` (PascalCase member
// name, e.g. "Active") - the two directions are asymmetric, never share one lookup table.
const STATUS_TO_ORDINAL: Record<ChatbotLifecycleStatus, number> = { borrador: 0, activo: 1, inactivo: 2 };
const STATUS_FROM_API: Record<string, ChatbotLifecycleStatus> = { Draft: "borrador", Active: "activo", Inactive: "inactivo" };

const PROVIDER_TO_ORDINAL: Record<LlmProvider, number> = { anthropic: 0, openai: 1, local: 2 };
const PROVIDER_FROM_API: Record<string, LlmProvider> = { Anthropic: "anthropic", OpenAi: "openai", Local: "local" };

const DOC_STATUS_FROM_API: Record<string, DocStatus> = { Ok: "ok", Pending: "pending" };

// Backend only has a single "Owner" role (ordinal 0) - the gendered "Propietario"/
// "Propietaria" split is presentation-only on this side (see ADR-15 in the backend's
// ADR.md), so an Owner always reads back as "Propietario"; writing either gendered form
// maps back to the same ordinal.
const ROLE_TO_ORDINAL: Record<TeamRole, number> = { Propietario: 0, Propietaria: 0, Editor: 1, Lector: 2 };
const ROLE_FROM_API: Record<string, TeamRole> = { Owner: "Propietario", Editor: "Editor", Reader: "Lector" };

function toApiSettings(settings: ChatbotSettings): UpdateChatbotSettingsRequest {
  return {
    socratic: settings.socratic,
    explicaMas: settings.explicaMas,
    provider: PROVIDER_TO_ORDINAL[settings.provider],
    model: settings.model
  };
}

// Backend sends a raw Date for "updated" (no relative-time label like the old mock's
// scripted "hace 2 días" copy) - this is a plain, honest date format, not an attempt to
// reproduce that flavor text. A nicer relative-time formatter is a follow-up, not this swap.
function formatDate(value: Date | string | undefined): string {
  if (!value) return "";
  const date = typeof value === "string" ? new Date(value) : value;
  return Number.isNaN(date.getTime()) ? "" : date.toLocaleDateString("es-ES");
}

function toChatExample(row: { question?: string; answer?: string; citation?: string; moreExplanation?: string; socraticVariant?: string }): ChatExample {
  return { q: row.question ?? "", a: row.answer ?? "", cite: row.citation ?? "", more: row.moreExplanation ?? "", socratic: row.socraticVariant ?? "" };
}

function toDocument(row: ChatbotDocumentResponse): ChatbotDocument {
  return {
    id: row.id,
    name: row.name ?? "",
    origin: row.origin ?? "",
    status: DOC_STATUS_FROM_API[row.status ?? "Ok"] ?? "ok",
    statusLabel: row.statusLabel ?? "",
    updated: formatDate(row.updated),
    url: row.url
  };
}

function toTeamMember(row: TeamMemberRow): TeamMember {
  return { userId: row.userId ?? "", role: ROLE_FROM_API[row.role ?? "Owner"] ?? "Lector" };
}

function toGapQuestion(row: GapQuestionRow): GapQuestion {
  return { q: row.question ?? "", count: row.count ?? 0 };
}

function toStatTile(row: StatTileRow): StatTile {
  return { label: row.label ?? "", value: row.value ?? "", unit: row.unit ?? "", bar: row.bar ?? 0 };
}

/**
 * Merges the list, detail, and chat-page endpoints into the flat `Chatbot` shape the rest
 * of the app already expects (N+1+1 fetch per FindAll - fine at this scale, revisit with a
 * batching/GraphQL-style endpoint if the chatbot count grows enough to matter).
 */
function toChatbot(summary: ChatbotSummaryResponse, detail: ChatbotDetailResponse, chatPage: ChatPageResponse): Chatbot {
  return {
    id: summary.id ?? "",
    label: summary.label ?? "",
    description: summary.description ?? "",
    status: STATUS_FROM_API[summary.status ?? "Draft"] ?? "borrador",
    context: detail.context ?? "",
    opening: chatPage.opening ? toChatExample(chatPage.opening) : null,
    settings: {
      socratic: detail.socratic ?? false,
      explicaMas: detail.explicaMas ?? false,
      provider: PROVIDER_FROM_API[detail.provider ?? "Anthropic"] ?? "anthropic",
      model: detail.model ?? ""
    },
    docs: (detail.docs ?? []).map(toDocument),
    gaps: (detail.gaps ?? []).map(toGapQuestion),
    topQuestions: (detail.topQuestions ?? []).map(toGapQuestion),
    team: (detail.team ?? []).map(toTeamMember),
    stats: (detail.stats ?? []).map(toStatTile),
    archived: summary.archived ?? false,
    iconUrl: summary.iconUrl
  };
}

export class ApiChatbotsRepository implements IChatbotsRepository {
  private readonly client: GeneratedNakamaClient;

  constructor(client: GeneratedNakamaClient) {
    this.client = client;
  }

  async FindAll(): Promise<ChatbotMap> {
    const summaries = await this.client.chatbotsAll();
    const entries = await Promise.all(
      summaries.map(async (summary) => {
        const id = summary.id ?? "";
        const [detail, chatPage] = await Promise.all([this.client.chatbotsGET(id), this.client.chatPage(id)]);
        return [id, toChatbot(summary, detail, chatPage)] as const;
      })
    );
    return Object.fromEntries(entries);
  }

  async Create(input: CreateChatbotInput): Promise<Chatbot> {
    const { name, dept, description, status, settings, iconUrl } = input;
    const id = crypto.randomUUID();
    const trimmedDescription = description.trim();
    const fallbackDescription = dept
      ? `Chatbot de equipo para ${dept}. Todavía sin documentos.`
      : "Chatbot recién creado. Todavía sin documentos.";
    const finalDescription = trimmedDescription || fallbackDescription;

    await this.client.chatbotsPUT(id, {
      label: name,
      description: finalDescription,
      status: STATUS_TO_ORDINAL[status],
      ...toApiSettings(settings)
    } satisfies CreateChatbotRequest);

    if (iconUrl) {
      await this.client.iconPOST(id, await dataUrlToFileParameter(iconUrl, "icon"));
    }

    // Built locally instead of a follow-up GET - same pattern the mock used, and the
    // backend response body is empty (201 Created) anyway, so there's nothing to read back.
    return {
      id,
      label: name,
      description: finalDescription,
      status,
      context:
        "Chatbot recién creado" +
        (dept ? ` para el equipo de <strong>${dept}</strong>` : "") +
        ". Todavía no tiene documentos — añade contenido en la pestaña Documentos para que pueda empezar a responder.",
      opening: null,
      settings,
      iconUrl,
      docs: [],
      gaps: [],
      topQuestions: [],
      team: [],
      stats: [
        { label: "Tiempo hasta productividad", value: "—", unit: "sin datos todavía", bar: 0 },
        { label: "Preguntas directas a seniors / semana", value: "—", unit: "sin datos todavía", bar: 0 },
        { label: "Respuestas útiles del bot", value: "—", unit: "sin datos todavía", bar: 0 },
        { label: "Documentos ingestados", value: "0", unit: "objetivo >100", bar: 0 }
      ]
    };
  }

  async Archive(id: string): Promise<void> {
    await this.client.archive(id);
  }

  async UpdateSettings(id: string, patch: Partial<ChatbotSettings>): Promise<void> {
    // The backend has no partial-update endpoint - it replaces the whole settings object -
    // so a partial patch needs the current settings first to merge into. Costs one extra
    // GET; acceptable at this stage, same "don't over-engineer batching yet" call as FindAll.
    const current = await this.client.chatbotsGET(id);
    const merged: ChatbotSettings = {
      socratic: patch.socratic ?? current.socratic ?? false,
      explicaMas: patch.explicaMas ?? current.explicaMas ?? false,
      provider: patch.provider ?? (PROVIDER_FROM_API[current.provider ?? "Anthropic"] ?? "anthropic"),
      model: patch.model ?? current.model ?? ""
    };
    await this.client.settings(id, toApiSettings(merged));
  }

  async UpdateStatus(id: string, status: ChatbotLifecycleStatus): Promise<void> {
    await this.client.status(id, { status: STATUS_TO_ORDINAL[status] } satisfies UpdateChatbotStatusRequest);
  }

  async UpdateIcon(id: string, iconUrl: string | undefined): Promise<void> {
    if (iconUrl) {
      await this.client.iconPOST(id, await dataUrlToFileParameter(iconUrl, "icon"));
    } else {
      await this.client.iconDELETE(id);
    }
  }

  async AddDocument(id: string, file: File, name: string, origin: string): Promise<void> {
    await this.client.documentsPUT(id, crypto.randomUUID(), { data: file, fileName: file.name }, name, origin);
  }

  async RemoveDocument(id: string, index: number): Promise<void> {
    await this.client.documentsDELETE(id, index);
  }

  async AddTeamMember(id: string, member: TeamMember): Promise<void> {
    await this.client.teamPOST(id, { userId: member.userId, role: ROLE_TO_ORDINAL[member.role] });
  }

  async RemoveTeamMember(id: string, index: number): Promise<void> {
    // The mock pre-checked "is this the owner" locally before removing; without a locally
    // cached team list here, that guard is enforced server-side only now
    // (ChatbotErrors.OwnerCannotBeRemoved -> 422) - the domain invariant still holds, it's
    // just checked in one place instead of two.
    await this.client.teamDELETE(id, index);
  }
}
