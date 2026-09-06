using Nakama.Chatbots.Domain;
using Nakama.Shared.Infrastructure.Persistence.Nh.UserTypes;
using NHibernate;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

/// <summary>
/// Write-side mapping for the <see cref="Chatbot"/> aggregate. Scalar columns map 1:1;
/// <see cref="ChatbotSettings"/> and the private <c>_documents</c>/<c>_team</c> backing fields
/// map as JSON columns via <see cref="JsonUserType{T}"/> (agents.md §6 "tipos JSON complejos"),
/// avoiding a full component/bag mapping for this scope.
/// </summary>
public sealed class ChatbotMapping : ClassMapping<Chatbot>
{
    public ChatbotMapping()
    {
        Table("chatbots");

        Id(x => x.Id, m =>
        {
            m.Column("key");
            m.Generator(Generators.Assigned);
            m.Type<ValueObjectUserType<ChatbotKey>>();
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
