using Nakama.Chatbots.Domain;
using Nakama.Shared.Infrastructure.Persistence.Nh.UserTypes;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

/// <summary>
/// Write-side mapping for the <see cref="Conversation"/> aggregate. References
/// <see cref="Chatbot"/> only by <see cref="ChatbotKey"/> (see <c>Conversation.BotKey</c>) - no
/// NHibernate association is declared, matching the domain rule that this is its own aggregate.
/// </summary>
public sealed class ConversationMapping : ClassMapping<Conversation>
{
    public ConversationMapping()
    {
        Table("conversations");

        Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
            m.Type<ValueObjectUserType<ConversationId>>();
        });

        Property(x => x.BotKey, m =>
        {
            m.Column("bot_key");
            m.Type<ValueObjectUserType<ChatbotKey>>();
        });

        Property(x => x.Title);
        Property(x => x.StartedAt);

        Property("_exchanges", m =>
        {
            m.Column("exchanges_json");
            m.Access(Accessor.Field);
            m.Type<JsonUserType<List<ConversationExchange>>>();
        });
    }
}
