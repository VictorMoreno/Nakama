using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Persistence.Nh;
using Nakama.UserActivity.Domain;

namespace Nakama.UserActivity.Infrastructure;

public sealed class NhUserActivityRepository(IWriteSessionFactory writeSessions)
    : NhRepositoryBase<UserActivityEntry, Guid>(writeSessions), IUserActivityRepository;
