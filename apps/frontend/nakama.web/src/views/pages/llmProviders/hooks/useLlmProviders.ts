import { useEffect, useState } from "react";
import type { ILlmProvidersRepository } from "../../../../modules/llmProviders/domain/ILlmProvidersRepository";
import type { LlmModelOption, LlmProviderConfig, LlmProviderKind } from "../../../../modules/llmProviders/domain/LlmProviderConfig";
import { Services } from "../../../../shared/infrastructure/injection/Services";
import { useInjection } from "../../../../shared/infrastructure/injection/useInjection";
import { useToast } from "../../../shared/contexts/ToastContext";

export const KNOWN_PROVIDERS: { value: LlmProviderKind; label: string }[] = [{ value: "anthropic", label: "Anthropic (Claude)" }];

function emptyConfig(provider: LlmProviderKind): LlmProviderConfig {
  return { provider, configured: false, enabled: false, models: [], updatedAt: "" };
}

export function useLlmProviders() {
  const repo = useInjection<ILlmProvidersRepository>(Services.LlmProvidersRepository.id);
  const { showToast } = useToast();
  const [providers, setProviders] = useState<LlmProviderConfig[]>([]);
  const [loading, setLoading] = useState(true);
  const [activeTab, setActiveTab] = useState<LlmProviderKind>(KNOWN_PROVIDERS[0].value);

  async function reload() {
    const rows = await repo.FindAll();
    setProviders(rows);
    setLoading(false);
  }

  useEffect(() => {
    void reload();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [repo]);

  function configFor(provider: LlmProviderKind): LlmProviderConfig {
    return providers.find((p) => p.provider === provider) ?? emptyConfig(provider);
  }

  async function register(provider: LlmProviderKind, apiKey: string, enabled: boolean, models: LlmModelOption[]) {
    try {
      await repo.Register(provider, apiKey, enabled, models);
      await reload();
      showToast("Proveedor actualizado");
    } catch {
      showToast("No se pudo guardar el proveedor.");
    }
  }

  return { providers, loading, configFor, register, activeTab, setActiveTab };
}
