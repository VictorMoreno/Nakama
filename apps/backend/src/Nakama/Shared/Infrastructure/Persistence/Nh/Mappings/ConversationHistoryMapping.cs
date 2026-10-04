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
        this.Table("read_conversation_history");
        this.Lazy(false);

        this.Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
        });

        this.Property(x => x.Entries, m => { m.Column(c => { c.Name("entries_json"); c.SqlType("text"); }); m.Type<JsonUserType<List<ConversationSummaryRow>>>(); });
    }
}
