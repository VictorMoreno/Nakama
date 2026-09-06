namespace Nakama.Shared.Domain;

/// <summary>
/// Shared contract per agents.md §5. Factory methods take this instead of calling
/// <c>Guid.NewGuid()</c> directly so identity generation stays deterministic and testable.
/// Returns <see cref="string"/> (not <see cref="Guid"/>) because every aggregate identity in
/// Nakama is a value object wrapping a string (e.g. <c>ChatbotKey</c>), consumed directly as
/// <c>ChatbotKey.From(ids.NewId())</c>.
/// </summary>
public interface IUuidGenerator
{
    string NewId();
}
