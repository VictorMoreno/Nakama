using Nakama.Analytics.Domain;
using Nakama.Analytics.Domain.Errors;
using Nakama.Shared.Domain;

namespace Nakama.Analytics.Application.Finder;

public static class AnalyticsFinder
{
    public sealed record Query : IQuery<AnalyticsResponse>;

    public sealed class Handler(IAnalyticsRepository analyticsRepository) : IQueryHandler<Query, AnalyticsResponse>
    {
        public async Task<AnalyticsResponse> Handle(Query query, CancellationToken ct)
        {
            Domain.Analytics analytics = await analyticsRepository.Find(ct) ?? throw AnalyticsErrors.NotFound;
            return (AnalyticsResponse)analytics;
        }
    }
}
