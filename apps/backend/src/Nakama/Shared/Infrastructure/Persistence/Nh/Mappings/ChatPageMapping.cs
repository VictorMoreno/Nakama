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
        Lazy(false);

        Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
        });

        Property(x => x.Label);
        Property(x => x.Context, m => m.Column(c => c.SqlType("text")));
        Property(x => x.Socratic);
        Property(x => x.ExplicaMas);
        Property(x => x.Provider);

        Property(x => x.Opening, m => { m.Column(c => { c.Name("opening_json"); c.SqlType("text"); }); m.Type<JsonUserType<ChatExampleRow>>(); });
        Property(x => x.Chips, m => { m.Column(c => { c.Name("chips_json"); c.SqlType("text"); }); m.Type<JsonUserType<List<ChatExampleRow>>>(); });
    }
}
