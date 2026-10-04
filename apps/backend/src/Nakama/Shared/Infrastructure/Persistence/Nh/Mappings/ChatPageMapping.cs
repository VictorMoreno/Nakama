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
        this.Table("read_chat_page");
        this.Lazy(false);

        this.Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
        });

        this.Property(x => x.Label);
        this.Property(x => x.Context, m => m.Column(c => c.SqlType("text")));
        this.Property(x => x.Socratic);
        this.Property(x => x.ExplicaMas);
        this.Property(x => x.Provider);

        this.Property(x => x.Opening, m => { m.Column(c => { c.Name("opening_json"); c.SqlType("text"); }); m.Type<JsonUserType<ChatExampleRow>>(); });
    }
}
