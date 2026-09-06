using Nakama.Shared.Domain;

namespace Nakama.Shared.Infrastructure;

public sealed class SystemClock : ISystemClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
