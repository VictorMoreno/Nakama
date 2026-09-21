using Nakama.Chatbots.Domain;
using Nakama.Shared.Infrastructure.Persistence.Nh.UserTypes;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

public sealed class ChatbotMapping : ClassMapping<Chatbot>
{
    public ChatbotMapping()
    {
        Table("chatbots");
        Lazy(false);

        Id(x => x.Id, m =>
        {
            m.Column("key");
            m.Generator(Generators.Assigned);
            m.Type<GuidValueObjectUserType<ChatbotId>>();
        });

        Property(x => x.Label);
        Property(x => x.Description, m => m.Column(c => c.SqlType("text")));
        Property(x => x.Status);
        Property(x => x.Context, m => m.Column(c => c.SqlType("text")));
        Property(x => x.General);
        Property(x => x.Archived);
        Property(x => x.IconUrl);
        Property(x => x.LastUsed);
        Property(x => x.Settings, m =>
        {
            m.Type<JsonUserType<ChatbotSettings>>();
            m.Column(c => c.SqlType("text"));
        });

        Property("_documents", m =>
        {
            m.Column(c => { c.Name("documents_json"); c.SqlType("text"); });
            m.Access(Accessor.Field);
            m.Type<JsonUserType<List<ChatbotDocument>>>();
        });

        Property("_team", m =>
        {
            m.Column(c => { c.Name("team_json"); c.SqlType("text"); });
            m.Access(Accessor.Field);
            m.Type<JsonUserType<List<TeamMember>>>();
        });
    }
}
