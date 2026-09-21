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
        Lazy(false);

        Id(x => x.Id, m =>
        {
            m.Column("key");
            m.Generator(Generators.Assigned);
        });

        Property(x => x.ActiveBotsCount);
        Property(x => x.AnsweredThisWeek);
        Property(x => x.UsageByBot, m => { m.Column(c => { c.Name("usage_by_bot_json"); c.SqlType("text"); }); m.Type<JsonUserType<List<UsageByBotRow>>>(); });
    }
}
