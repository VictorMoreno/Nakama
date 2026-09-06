using Nakama.Shared.Domain;

namespace Nakama.Analytics.Domain;

/// <summary>
/// Read entity behind the <c>/analytics</c> route - replaces the frontend's static
/// <c>analyticsData.ts</c> mock with data aggregated from real domain events. Not an aggregate:
/// no invariant, no identity of its own - a singleton row (<see cref="Key"/> is always
/// <c>"global"</c>) computed entirely as a derived projection across
/// <c>Chatbot</c>/<c>Conversation</c>/<c>Formation</c> events, per the plan's explicit
/// "Analytics is not an aggregate" call. Still implements <see cref="IEntity"/> and lives in
/// its own module with the same Domain/Application/Infrastructure shape as any other read
/// module, so it's fed and queried the same way as the rest.
/// </summary>
public partial class Analytics : IEntity
{
    protected Analytics()
    {
    } // NHibernate

    public const string SingletonKey = "global";

    public virtual string Key { get; protected set; } = SingletonKey;

    public virtual int ActiveBotsCount { get; protected set; }

    public virtual int AnsweredThisWeek { get; protected set; }

    public virtual List<UsageByBotRow> UsageByBot { get; protected set; } = [];
}
