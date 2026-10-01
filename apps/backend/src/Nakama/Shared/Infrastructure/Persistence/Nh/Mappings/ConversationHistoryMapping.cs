using Nakama.ConversationHistory.Domain;
using Nakama.Shared.Infrastructure.Persistence.Nh.UserTypes;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

using ConversationHistory = Nakama.ConversationHistory.Domain.ConversationHistory;

public sealed class ConversationHistoryMapping : ClassMapping<ConversationHistory>
{
    public ConversationHistoryMapping()
    {
        Table("read_conversation_history");
        Lazy(false);

        Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
        });

        Property(x => x.Entries, m => { m.Column(c => { c.Name("entries_json"); c.SqlType("text"); }); m.Type<JsonUserType<List<ConversationSummaryRow>>>(); });
    }
}
