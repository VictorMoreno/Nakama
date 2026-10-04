using Nakama.Formations.Domain;
using Nakama.Shared.Infrastructure.Persistence.Nh.UserTypes;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

public sealed class FormationMapping : ClassMapping<Formation>
{
    public FormationMapping()
    {
        this.Table("formations");
        this.Lazy(false);

        this.Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
            m.Type<GuidValueObjectUserType<FormationId>>();
        });

        this.Property(x => x.Title);
        this.Property(x => x.Audience);
        this.Property(x => x.Description, m => m.Column(c => c.SqlType("text")));
        this.Property(x => x.Status);
        this.Property(x => x.LinkedChatbotKey, m => m.Column("linked_chatbot_key"));
        this.Property(x => x.Archived);
        this.Property(x => x.IconUrl);

        this.Property("_modules", m =>
        {
            m.Column(c => { c.Name("modules_json"); c.SqlType("text"); });
            m.Access(Accessor.Field);
            m.Type<JsonUserType<List<FormationModule>>>();
        });
    }
}
