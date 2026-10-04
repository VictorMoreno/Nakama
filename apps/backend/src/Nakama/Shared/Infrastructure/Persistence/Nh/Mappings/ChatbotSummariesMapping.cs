using Nakama.ChatbotSummaries.Domain;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

public sealed class ChatbotSummariesMapping : ClassMapping<ChatbotSummary>
{
    public ChatbotSummariesMapping()
    {
        this.Table("read_chatbot_summaries");
        this.Lazy(false);

        this.Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
        });

        this.Property(x => x.Label);
        this.Property(x => x.Description, m => m.Column(c => c.SqlType("text")));
        this.Property(x => x.Status);
        this.Property(x => x.DocsCount);
        this.Property(x => x.TeamCount);
        this.Property(x => x.General);
        this.Property(x => x.Archived);
        this.Property(x => x.IconUrl);
    }
}
