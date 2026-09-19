using Nakama.Chatbots.Domain;
using Nakama.Shared.Infrastructure.Persistence.Nh.UserTypes;
using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

public sealed class ChatbotMapping : ClassMapping<Chatbot>
{
    public ChatbotMapping()
    {
        Table("chatbots");

        Id(x => x.Id, m =>
        {
            m.Column("key");
            m.Generator(Generators.Assigned);
            m.Type<GuidValueObjectUserType<ChatbotId>>();
        });

        Property(x => x.Label);
        Property(x => x.Description);
        Property(x => x.Status, m => m.Type(NHibernateUtil.Enum(typeof(ChatbotStatus))));
        Property(x => x.Context);
        Property(x => x.General);
        Property(x => x.Archived);
        Property(x => x.IconUrl);
        Property(x => x.LastUsed);
        Property(x => x.Settings, m => m.Type<JsonUserType<ChatbotSettings>>());

        Property("_documents", m =>
        {
            m.Column("documents_json");
            m.Access(Accessor.Field);
            m.Type<JsonUserType<List<ChatbotDocument>>>();
        });

        Property("_team", m =>
        {
            m.Column("team_json");
            m.Access(Accessor.Field);
            m.Type<JsonUserType<List<TeamMember>>>();
        });
    }
}
