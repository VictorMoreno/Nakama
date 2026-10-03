import type { LlmModelOption, LlmProviderConfig, LlmProviderKind } from "./LlmProviderConfig";

export interface ILlmProvidersRepository {
  FindAll(): Promise<LlmProviderConfig[]>;

  Register(provider: LlmProviderKind, apiKey: string, enabled: boolean, models: LlmModelOption[]): Promise<void>;
}
