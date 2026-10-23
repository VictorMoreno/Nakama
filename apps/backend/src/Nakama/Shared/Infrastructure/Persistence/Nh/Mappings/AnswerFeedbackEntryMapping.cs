using Nakama.AnswerFeedback.Domain;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

public sealed class AnswerFeedbackEntryMapping : ClassMapping<AnswerFeedbackEntry>
{
    public AnswerFeedbackEntryMapping()
    {
        this.Table("read_answer_feedback");
        this.Lazy(false);

        this.Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
        });

        this.Property(x => x.BotKey);
        this.Property(x => x.Question, m => m.Column(c => c.SqlType("text")));
        this.Property(x => x.Useful);
        this.Property(x => x.RatedAt);
    }
}
