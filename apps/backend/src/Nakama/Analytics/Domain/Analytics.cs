using Nakama.Shared.Domain;

namespace Nakama.Analytics.Domain;

public partial class Analytics : IEntity
{
    protected Analytics()
    {
    } // NHibernate

    public const string SingletonKey = "global";

    public virtual string Id { get; protected set; } = SingletonKey;

    public virtual int ActiveBotsCount { get; protected set; }

    public virtual int AnsweredThisWeek { get; protected set; }

    public virtual List<UsageByBotRow> UsageByBot { get; protected set; } = [];
}
