using Nakama.Shared.Domain;

namespace Nakama.UserActivity.Domain.Errors;

public sealed class UserActivityInvalidDaysError(int maxDays)
    : DomainError("nakama.user_activity.invalid_days", $"The number of days must be between 1 and {maxDays}.");

public static class UserActivityErrors
{
    public static DomainError InvalidDays(int maxDays) => new UserActivityInvalidDaysError(maxDays);
}
