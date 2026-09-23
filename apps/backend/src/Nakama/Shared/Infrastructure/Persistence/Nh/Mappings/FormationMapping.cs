using Nakama.Formations.Domain;
using Nakama.Shared.Infrastructure.Persistence.Nh.UserTypes;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

public sealed class FormationMapping : ClassMapping<Formation>
{
    public FormationMapping()
    {
        Table("formations");
        Lazy(false);

        Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
            m.Type<GuidValueObjectUserType<FormationId>>();
        });

        Property(x => x.Title);
        Property(x => x.Audience);
        Property(x => x.Description, m => m.Column(c => c.SqlType("text")));
        Property(x => x.Status);
        Property(x => x.LinkedChatbotKey, m => m.Column("linked_chatbot_key"));
        Property(x => x.Archived);
        Property(x => x.IconUrl);
        Property(x => x.MyProgress, m =>
        {
            m.Type<JsonUserType<LearnerProgress>>();
            m.Column(c => c.SqlType("text"));
        });

        Property("_modules", m =>
        {
            m.Column(c => { c.Name("modules_json"); c.SqlType("text"); });
            m.Access(Accessor.Field);
            m.Type<JsonUserType<List<FormationModule>>>();
        });

        Property("_team", m =>
        {
            m.Column(c => { c.Name("team_json"); c.SqlType("text"); });
            m.Access(Accessor.Field);
            m.Type<JsonUserType<List<FormationMember>>>();
        });
    }
}
