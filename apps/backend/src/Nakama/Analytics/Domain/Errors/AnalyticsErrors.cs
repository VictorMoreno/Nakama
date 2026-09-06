using Nakama.Shared.Domain;

namespace Nakama.Analytics.Domain.Errors;

public sealed class AnalyticsNotFoundError()
    : DomainError("nakama.analytics.not_found", "Analytics not found.");

public static class AnalyticsErrors
{
    public static DomainError NotFound { get; } =  new AnalyticsNotFoundError();
}
