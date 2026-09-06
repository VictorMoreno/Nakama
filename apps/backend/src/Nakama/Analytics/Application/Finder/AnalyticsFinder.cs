using MediatR;
using Nakama.Analytics.Domain;
using Nakama.Analytics.Domain.Errors;

namespace Nakama.Analytics.Application.Finder;

public static class AnalyticsFinder
{
    public sealed record Query : IRequest<AnalyticsResponse>;

    public sealed class Handler(IAnalyticsRepository analyticsRepository) : IRequestHandler<Query, AnalyticsResponse>
    {
        public async Task<AnalyticsResponse> Handle(Query query, CancellationToken ct)
        {
            Domain.Analytics analytics = await analyticsRepository.FindSingleton(ct) ?? throw AnalyticsErrors.NotFound;
            return (AnalyticsResponse)analytics;
        }
    }
}
