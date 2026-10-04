using Nakama.UserActivity.Domain;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

public sealed class UserActivityEntryMapping : ClassMapping<UserActivityEntry>
{
    public UserActivityEntryMapping()
    {
        this.Table("read_user_activity");
        this.Lazy(false);

        this.Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
        });

        this.Property(x => x.UserId);
        this.Property(x => x.Type);
        this.Property(x => x.OccurredAt);
    }
}
