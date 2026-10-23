using Nakama.Analytics.Domain;
using Nakama.Analytics.Domain.Errors;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.UserActivity.Application;
using Nakama.UserActivity.Application.Finder;
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
        IQueryBus queryBus) : IService
    {
        private const int ActiveUsersWindowDays = 7;

        public async Task<AnalyticsResponse> Execute(CancellationToken cancellationToken)
        {
            Domain.Analytics analytics = await repository.Find(cancellationToken) ?? throw AnalyticsErrors.NotFound;
            UserActivityResponse activity = await queryBus.Ask(new UserActivityFinder.Query(ActiveUsersWindowDays), cancellationToken);
            int totalUsersCount = await usersRepository.Count(new UserListingSpecification(), cancellationToken);

            return analytics.ToResponse(activity.ActiveLast7Days, totalUsersCount);
        }
    }
}
