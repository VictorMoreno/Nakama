using Nakama.ChatbotSummaries.Domain;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

public sealed class ChatbotSummariesMapping : ClassMapping<ChatbotSummary>
{
    public ChatbotSummariesMapping()
    {
        Table("read_chatbot_summaries");
        Lazy(false);

        Id(x => x.Id, m =>
        {
            m.Column("key");
            m.Generator(Generators.Assigned);
        });

        Property(x => x.Label);
        Property(x => x.Description, m => m.Column(c => c.SqlType("text")));
        Property(x => x.Status);
        Property(x => x.LastUsed);
        Property(x => x.DocsCount);
        Property(x => x.TeamCount);
        Property(x => x.General);
        Property(x => x.Archived);
        Property(x => x.IconUrl);
    }
}
