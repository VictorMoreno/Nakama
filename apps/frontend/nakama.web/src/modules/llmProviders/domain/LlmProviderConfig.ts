export type LlmProviderKind = "anthropic" | "openai" | "local";

export interface LlmModelOption {
  id: string;
  label: string;
}

export interface LlmProviderConfig {
  provider: LlmProviderKind;
  configured: boolean;
  enabled: boolean;
  models: LlmModelOption[];
  updatedAt: string;
}
