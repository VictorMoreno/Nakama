using Nakama.Shared.Infrastructure.Persistence.Nh.UserTypes;
using Nakama.Users.Domain;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

public sealed class UserMapping : ClassMapping<User>
{
    public UserMapping()
    {
        Table("users");
        Lazy(false);

        Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
            m.Type<GuidValueObjectUserType<UserId>>();
        });

        Property(x => x.Email);
        Property(x => x.NormalizedEmail);
        Property(x => x.Name);
        Property(x => x.IsAdmin);
        Property(x => x.PasswordHash);
        Property(x => x.LastActiveAt);
        Property(x => x.CreatedAt);
    }
}
