using Nakama.Analytics.Domain;
using Nakama.Analytics.Domain.Errors;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Users.Domain;

namespace Nakama.Analytics.Application.Finder;

public static class AnalyticsFinder
{
    public sealed record Query : IQuery<AnalyticsResponse>;

    public sealed class Handler(Finder finder) : IQueryHandler<Query, AnalyticsResponse>
    {
        public async Task<AnalyticsResponse> Handle(Query query, CancellationToken ct) => await finder.Execute(ct);
    }

    public sealed class Finder(
        IAnalyticsRepository repository,
        IUsersRepository usersRepository,
        ISystemClock systemClock) : IService
    {
        public async Task<AnalyticsResponse> Execute(CancellationToken cancellationToken)
        {
            var analytics = await repository.Find(cancellationToken) ?? throw AnalyticsErrors.NotFound;
            var activeUsersCount =
                await usersRepository.CountActiveSince(systemClock.UtcNow.AddDays(-7), cancellationToken);
            var totalUsersCount = await usersRepository.CountAll(cancellationToken);

            return analytics.ToResponse(activeUsersCount, totalUsersCount);
        }
    }
}