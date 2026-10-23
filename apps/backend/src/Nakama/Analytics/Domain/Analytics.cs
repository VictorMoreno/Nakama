using Nakama.Shared.Domain;

namespace Nakama.Analytics.Domain;

public partial class Analytics : IEntity
{
    protected Analytics()
    {
    } // NHibernate

    public const string SingletonKey = "global";

    public string Id { get; protected set; } = SingletonKey;

    public int ActiveBotsCount { get; protected set; }

    public int UsefulFeedbackCount { get; protected set; }

    public int TotalFeedbackCount { get; protected set; }
}
