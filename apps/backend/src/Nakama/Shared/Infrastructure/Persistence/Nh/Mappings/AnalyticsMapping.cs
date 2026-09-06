using Nakama.Analytics.Domain;
using Nakama.Shared.Infrastructure.Persistence.Nh.UserTypes;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

using Analytics = Nakama.Analytics.Domain.Analytics;

public sealed class AnalyticsMapping : ClassMapping<Analytics>
{
    public AnalyticsMapping()
    {
        Table("read_analytics");

        Id(x => x.Key, m => m.Generator(Generators.Assigned));

        Property(x => x.ActiveBotsCount);
        Property(x => x.AnsweredThisWeek);
        Property(x => x.UsageByBot, m => { m.Column("usage_by_bot_json"); m.Type<JsonUserType<List<UsageByBotRow>>>(); });
    }
}
