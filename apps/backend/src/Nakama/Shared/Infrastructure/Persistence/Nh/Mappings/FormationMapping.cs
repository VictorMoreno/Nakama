using Nakama.Formations.Domain;
using Nakama.Shared.Infrastructure.Persistence.Nh.UserTypes;
using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

/// <summary>
/// Write-side mapping for the <see cref="Formation"/> aggregate. <c>LinkedChatbotKey</c> maps
/// as a plain string column - it's a cross-context id, never a NHibernate association, per
/// agents.md §1 (a bounded context must not reference another context's Domain).
/// </summary>
public sealed class FormationMapping : ClassMapping<Formation>
{
    public FormationMapping()
    {
        Table("formations");

        Id(x => x.Id, m =>
        {
            m.Column("key");
            m.Generator(Generators.Assigned);
            m.Type<ValueObjectUserType<FormationKey>>();
        });

        Property(x => x.Title);
        Property(x => x.Audience);
        Property(x => x.Description);
        Property(x => x.Status, m => m.Type(NHibernateUtil.Enum(typeof(FormationStatus))));
        Property(x => x.LinkedChatbotKey, m => m.Column("linked_chatbot_key"));
        Property(x => x.Archived);
        Property(x => x.IconUrl);
        Property(x => x.MyProgress, m => m.Type<JsonUserType<LearnerProgress>>());

        Property("_modules", m =>
        {
            m.Column("modules_json");
            m.Access(Accessor.Field);
            m.Type<JsonUserType<List<FormationModule>>>();
        });

        Property("_team", m =>
        {
            m.Column("team_json");
            m.Access(Accessor.Field);
            m.Type<JsonUserType<List<FormationMember>>>();
        });
    }
}
