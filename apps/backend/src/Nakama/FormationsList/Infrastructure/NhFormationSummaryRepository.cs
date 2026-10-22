using Nakama.FormationsList.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.FormationsList.Infrastructure;

public sealed class NhFormationSummaryRepository(INhUnitOfWork unitOfWork)
    : NhRepositoryBase<FormationSummary, Guid>(unitOfWork), IFormationSummaryRepository;
