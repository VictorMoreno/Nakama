using Nakama.Chatbots.Domain;
using Nakama.Shared.Infrastructure.Persistence.Nh.UserTypes;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

public sealed class ConversationMapping : ClassMapping<Conversation>
{
    public ConversationMapping()
    {
        Table("conversations");
        Lazy(false);

        Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
            m.Type<GuidValueObjectUserType<ConversationId>>();
        });

        Property(x => x.BotKey, m =>
        {
            m.Column("bot_key");
            m.Type<GuidValueObjectUserType<ChatbotId>>();
        });

        Property(x => x.UserId, m => m.Column("user_id"));

        Property(x => x.Title);
        Property(x => x.StartedAt);

        Property("_exchanges", m =>
        {
            m.Column(c => { c.Name("exchanges_json"); c.SqlType("text"); });
            m.Access(Accessor.Field);
            m.Type<JsonUserType<List<ConversationExchange>>>();
        });
    }
}
