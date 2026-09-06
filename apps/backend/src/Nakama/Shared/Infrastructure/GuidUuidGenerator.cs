using Nakama.Shared.Domain;

namespace Nakama.Shared.Infrastructure;

public sealed class GuidUuidGenerator : IUuidGenerator
{
    public string NewId() => Guid.NewGuid().ToString();
}
