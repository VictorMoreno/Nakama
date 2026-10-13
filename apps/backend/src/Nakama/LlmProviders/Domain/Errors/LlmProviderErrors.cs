using Nakama.Shared.Domain;

namespace Nakama.LlmProviders.Domain.Errors;

public sealed class LlmProviderNotConfiguredError(LlmProvider provider)
    : DomainError("nakama.llm_providers.not_configured", $"El proveedor '{provider}' no tiene una clave de API configurada o está deshabilitado.");

public sealed class LlmProviderRequestFailedError(LlmProvider provider, string reason)
    : DomainError("nakama.llm_providers.request_failed", $"La llamada al proveedor '{provider}' falló: {reason}");

public static class LlmProviderErrors
{
    public static DomainError NotConfigured(LlmProvider provider) => new LlmProviderNotConfiguredError(provider);

    public static DomainError RequestFailed(LlmProvider provider, string reason) => new LlmProviderRequestFailedError(provider, reason);
}
