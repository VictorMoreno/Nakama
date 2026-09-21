using Nakama.Analytics.Domain;
using Nakama.Analytics.Domain.Errors;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;

namespace Nakama.Analytics.Application.Finder;

public static class AnalyticsFinder
{
    public sealed record Query : IQuery<AnalyticsResponse>;

    public sealed class Handler(Finder finder) : IQueryHandler<Query, AnalyticsResponse>
    {
        public async Task<AnalyticsResponse> Handle(Query query, CancellationToken ct)
        {
            Domain.Analytics analytics = await finder.Execute(ct);
            return (AnalyticsResponse)analytics;
        }
    }

    public sealed class Finder(IAnalyticsRepository repository) : IService
    {
        public async Task<Domain.Analytics> Execute(CancellationToken cancellationToken) =>
            await repository.Find(cancellationToken) ?? throw AnalyticsErrors.NotFound;
    }
}
