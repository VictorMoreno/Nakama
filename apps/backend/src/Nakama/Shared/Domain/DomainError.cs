namespace Nakama.Shared.Domain;

/// <summary>
/// Base type for every business error, and the exception thrown for it - a concrete
/// error (e.g. <c>ChatbotErrors.OwnerCannotBeRemoved</c>) is thrown directly
/// (<c>throw ChatbotErrors.OwnerCannotBeRemoved;</c>), no separate wrapper exception.
/// Concrete errors live in each module's <c>Domain/Errors</c> folder with a stable,
/// project-prefixed code (<c>nakama.&lt;module&gt;...</c>, mirroring the origin guide's
/// <c>yq.spare_parts...</c>).
/// </summary>
public abstract class DomainError(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
