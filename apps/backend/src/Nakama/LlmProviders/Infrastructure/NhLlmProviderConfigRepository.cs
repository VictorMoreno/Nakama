using Nakama.LlmProviders.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.LlmProviders.Infrastructure;

public sealed class NhLlmProviderConfigRepository(IWriteSessionFactory writeSessions)
    : NhRepositoryBase<LlmProviderConfig, LlmProviderConfigId>(writeSessions), ILlmProviderConfigRepository;
