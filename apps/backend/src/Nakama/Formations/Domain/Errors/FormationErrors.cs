using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain.Errors;

public sealed class FormationModuleLockedByGatingError()
    : DomainError("nakama.formations.module_locked_by_gating", "El módulo anterior está bloqueado hasta completar el módulo previo.");

public sealed class FormationNotFoundError(string key)
    : DomainError("nakama.formations.not_found", $"Formation '{key}' not found.");

public sealed class FormationModuleNotFoundError(string moduleId, string formationKey)
    : DomainError("nakama.formations.module_not_found", $"Module '{moduleId}' not found in formation '{formationKey}'.");

public static class FormationErrors
{
    public static DomainError ModuleLockedByGating { get; } = new FormationModuleLockedByGatingError();

    public static DomainError NotFound(string key) => new FormationNotFoundError(key);

    public static DomainError ModuleNotFound(string moduleId, string formationKey) => new FormationModuleNotFoundError(moduleId, formationKey);
}
