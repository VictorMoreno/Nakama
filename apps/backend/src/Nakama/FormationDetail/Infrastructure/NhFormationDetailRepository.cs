using Nakama.FormationDetail.Domain;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;

namespace Nakama.FormationDetail.Infrastructure;

using FormationDetail = Nakama.FormationDetail.Domain.FormationDetail;

public sealed class NhFormationDetailRepository(INhUnitOfWork unitOfWork)
    : NhRepositoryBase<FormationDetail, Guid>(unitOfWork), IFormationDetailRepository;
