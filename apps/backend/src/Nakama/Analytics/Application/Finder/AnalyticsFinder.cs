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
            Domain.Analytics analytics = await repository.Find(cancellationToken) ?? throw AnalyticsErrors.NotFound;
            int activeUsersCount = await usersRepository.Count(
                new UserActiveSinceSpecification(systemClock.UtcNow.AddDays(-7)),
                cancellationToken);
            int totalUsersCount = await usersRepository.Count(new UserListingSpecification(), cancellationToken);

            return analytics.ToResponse(activeUsersCount, totalUsersCount);
        }
    }
}