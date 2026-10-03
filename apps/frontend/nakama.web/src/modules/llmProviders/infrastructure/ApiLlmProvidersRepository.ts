import type { GeneratedNakamaClient, LlmProviderConfigResponse } from "../../../shared/infrastructure/GeneratedNakamaClient";
import type { ILlmProvidersRepository } from "../domain/ILlmProvidersRepository";
import type { LlmModelOption, LlmProviderConfig, LlmProviderKind } from "../domain/LlmProviderConfig";

const PROVIDER_TO_ORDINAL: Record<LlmProviderKind, number> = { anthropic: 0, openai: 1, local: 2 };
const PROVIDER_FROM_API: Record<number, LlmProviderKind> = { 0: "anthropic", 1: "openai", 2: "local" };

function toLlmProviderConfig(row: LlmProviderConfigResponse): LlmProviderConfig {
  return {
    provider: PROVIDER_FROM_API[row.provider ?? 0] ?? "anthropic",
    configured: row.configured ?? false,
    enabled: row.enabled ?? false,
    models: (row.models ?? []).map((m) => ({ id: m.id ?? "", label: m.label ?? "" })),
    updatedAt: row.updatedAt ? new Date(row.updatedAt).toISOString() : ""
  };
}

export class ApiLlmProvidersRepository implements ILlmProvidersRepository {
  private readonly client: GeneratedNakamaClient;

  constructor(client: GeneratedNakamaClient) {
    this.client = client;
  }

  async FindAll(): Promise<LlmProviderConfig[]> {
    const rows = await this.client.llmProvidersAll();
    return rows.map(toLlmProviderConfig);
  }

  async Register(provider: LlmProviderKind, apiKey: string, enabled: boolean, models: LlmModelOption[]): Promise<void> {
    await this.client.llmProviders(PROVIDER_TO_ORDINAL[provider], { apiKey, enabled, models });
  }
}
