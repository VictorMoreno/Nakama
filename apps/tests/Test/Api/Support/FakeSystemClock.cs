using Nakama.Shared.Domain;

namespace Test.Api.Support;

public sealed class FakeSystemClock : ISystemClock
{
    public DateTime UtcNow { get; private set; } = DateTime.UtcNow;

    public void Reset() => this.UtcNow = DateTime.UtcNow;

    public void Advance(TimeSpan time) => this.UtcNow = this.UtcNow.Add(time);
}
