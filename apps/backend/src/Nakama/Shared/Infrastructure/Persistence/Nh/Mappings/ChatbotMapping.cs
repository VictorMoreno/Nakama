using Nakama.Chatbots.Domain;
using Nakama.Shared.Infrastructure.Persistence.Nh.UserTypes;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

public sealed class ChatbotMapping : ClassMapping<Chatbot>
{
    public ChatbotMapping()
    {
        this.Table("chatbots");
        this.Lazy(false);

        this.Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
            m.Type<GuidValueObjectUserType<ChatbotId>>();
        });

        this.Property(x => x.Label);
        this.Property(x => x.Description, m => m.Column(c => c.SqlType("text")));
        this.Property(x => x.Status);
        this.Property(x => x.Context, m => m.Column(c => c.SqlType("text")));
        this.Property(x => x.General);
        this.Property(x => x.Archived);
        this.Property(x => x.IconUrl);
        this.Property(x => x.LastUsed);
        this.Property(x => x.Settings, m =>
        {
            m.Type<JsonUserType<ChatbotSettings>>();
            m.Column(c => c.SqlType("text"));
        });

        this.Property("_documents", m =>
        {
            m.Column(c => { c.Name("documents_json"); c.SqlType("text"); });
            m.Access(Accessor.Field);
            m.Type<JsonUserType<List<ChatbotDocument>>>();
        });

        this.Property("_team", m =>
        {
            m.Column(c => { c.Name("team_json"); c.SqlType("text"); });
            m.Access(Accessor.Field);
            m.Type<JsonUserType<List<TeamMember>>>();
        });
    }
}
