using Nakama.UserActivity.Domain;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

public sealed class UserActivityEntryMapping : ClassMapping<UserActivityEntry>
{
    public UserActivityEntryMapping()
    {
        Table("read_user_activity");
        Lazy(false);

        Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
        });

        Property(x => x.UserId);
        Property(x => x.Type);
        Property(x => x.OccurredAt);
    }
}
