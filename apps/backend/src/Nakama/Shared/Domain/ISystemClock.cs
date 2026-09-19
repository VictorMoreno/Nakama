namespace Nakama.Shared.Domain;

public interface ISystemClock
{
    DateTime UtcNow { get; }
}
