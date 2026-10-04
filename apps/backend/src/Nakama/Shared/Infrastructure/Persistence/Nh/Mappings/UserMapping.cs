using Nakama.Shared.Infrastructure.Persistence.Nh.UserTypes;
using Nakama.Users.Domain;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

public sealed class UserMapping : ClassMapping<User>
{
    public UserMapping()
    {
        this.Table("users");
        this.Lazy(false);

        this.Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
            m.Type<GuidValueObjectUserType<UserId>>();
        });

        this.Property(x => x.Email);
        this.Property(x => x.NormalizedEmail);
        this.Property(x => x.Name);
        this.Property(x => x.IsAdmin);
        this.Property(x => x.PasswordHash);
        this.Property(x => x.LastActiveAt);
        this.Property(x => x.CreatedAt);
    }
}
