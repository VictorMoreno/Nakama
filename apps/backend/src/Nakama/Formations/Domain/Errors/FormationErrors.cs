using Nakama.Shared.Domain;

namespace Nakama.Formations.Domain.Errors;

public sealed class FormationModuleLockedByGatingError()
    : DomainError("nakama.formations.module_locked_by_gating", "El módulo anterior está bloqueado hasta completar el módulo previo.");

public sealed class FormationNotFoundError(string key)
    : DomainError("nakama.formations.not_found", $"Formation '{key}' not found.");

public sealed class FormationModuleNotFoundError(string moduleId, string formationKey)
    : DomainError("nakama.formations.module_not_found", $"Module '{moduleId}' not found in formation '{formationKey}'.");

public sealed class FormationAlreadyExistsError(string key)
    : DomainError("nakama.formations.already_exists", $"Formation '{key}' already exists.");

public sealed class FormationModuleAlreadyExistsError(string moduleId, string formationKey)
    : DomainError("nakama.formations.module_already_exists", $"Module '{moduleId}' already exists in formation '{formationKey}'.");

public static class FormationErrors
{
    public static DomainError ModuleLockedByGating { get; } = new FormationModuleLockedByGatingError();

    public static DomainError NotFound(FormationId id) => new FormationNotFoundError(id.ToString());

    public static DomainError ModuleNotFound(string moduleId, string formationKey) => new FormationModuleNotFoundError(moduleId, formationKey);

    public static DomainError AlreadyExists(string key) => new FormationAlreadyExistsError(key);

    public static DomainError ModuleAlreadyExists(string moduleId, string formationKey) => new FormationModuleAlreadyExistsError(moduleId, formationKey);
}
