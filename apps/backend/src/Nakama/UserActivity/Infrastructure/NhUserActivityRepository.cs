using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;
using Nakama.UserActivity.Domain;

namespace Nakama.UserActivity.Infrastructure;

public sealed class NhUserActivityRepository(INhUnitOfWork unitOfWork)
    : NhRepositoryBase<UserActivityEntry, Guid>(unitOfWork), IUserActivityRepository;
