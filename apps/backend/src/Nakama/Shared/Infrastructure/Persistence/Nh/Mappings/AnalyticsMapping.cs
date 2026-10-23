using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

using Analytics = Nakama.Analytics.Domain.Analytics;

public sealed class AnalyticsMapping : ClassMapping<Analytics>
{
    public AnalyticsMapping()
    {
        this.Table("read_analytics");
        this.Lazy(false);

        this.Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
        });

        this.Property(x => x.ActiveBotsCount);
    }
}
