using Nakama.LlmProviders.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.LlmProviders.Infrastructure;

public sealed class NhLlmProviderConfigRepository(INhUnitOfWork unitOfWork)
    : NhRepositoryBase<LlmProviderConfig, LlmProviderConfigId>(unitOfWork), ILlmProviderConfigRepository;
