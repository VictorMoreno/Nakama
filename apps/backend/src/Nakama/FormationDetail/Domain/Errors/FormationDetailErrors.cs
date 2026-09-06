using Nakama.Shared.Domain;

namespace Nakama.FormationDetail.Domain.Errors;

public sealed class FormationDetailNotFoundError(string key)
    : DomainError("nakama.formation_detail.not_found", $"Formation '{key}' not found.");

public static class FormationDetailErrors
{
    public static DomainError NotFound(string key) => new FormationDetailNotFoundError(key);
}
