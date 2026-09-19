using Nakama.ChatPage.Domain;
using Nakama.Shared.Infrastructure.Persistence.Nh.UserTypes;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

using ChatPage = Nakama.ChatPage.Domain.ChatPage;

public sealed class ChatPageMapping : ClassMapping<ChatPage>
{
    public ChatPageMapping()
    {
        Table("read_chat_page");

        Id(x => x.Id, m =>
        {
            m.Column("bot_key");
            m.Generator(Generators.Assigned);
        });

        Property(x => x.Label);
        Property(x => x.Context);
        Property(x => x.Socratic);
        Property(x => x.ExplicaMas);
        Property(x => x.Provider);

        Property(x => x.Opening, m => { m.Column("opening_json"); m.Type<JsonUserType<ChatExampleRow>>(); });
        Property(x => x.Chips, m => { m.Column("chips_json"); m.Type<JsonUserType<List<ChatExampleRow>>>(); });
        Property(x => x.History, m => { m.Column("history_json"); m.Type<JsonUserType<List<ConversationSummaryRow>>>(); });
    }
}
