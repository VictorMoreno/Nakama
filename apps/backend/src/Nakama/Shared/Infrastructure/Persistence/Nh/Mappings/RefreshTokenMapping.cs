using Nakama.Authentication.Domain;
using Nakama.Shared.Infrastructure.Persistence.Nh.UserTypes;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

public sealed class RefreshTokenMapping : ClassMapping<RefreshToken>
{
    public RefreshTokenMapping()
    {
        this.Table("refresh_tokens");
        this.Lazy(false);

        this.Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
            m.Type<GuidValueObjectUserType<RefreshTokenId>>();
        });

        this.Property(x => x.UserId, m => m.Column("user_id"));
        this.Property(x => x.TokenHash, m => m.Column("token_hash"));
        this.Property(x => x.ExpiresAt, m => m.Column("expires_at"));
        this.Property(x => x.CreatedAt, m => m.Column("created_at"));
        this.Property(x => x.RevokedAt, m => m.Column("revoked_at"));
        this.Property(x => x.ReplacedById, m => m.Column("replaced_by_id"));
    }
}
