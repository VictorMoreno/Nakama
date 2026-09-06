namespace Nakama.Shared.Domain;

/// <summary>
/// Shared contract per agents.md §5. Factory methods on aggregates (e.g. <c>Chatbot.Create</c>)
/// take this instead of calling <c>DateTime.UtcNow</c> directly so domain behavior stays
/// deterministic and testable.
/// </summary>
public interface ISystemClock
{
    DateTime UtcNow { get; }
}
