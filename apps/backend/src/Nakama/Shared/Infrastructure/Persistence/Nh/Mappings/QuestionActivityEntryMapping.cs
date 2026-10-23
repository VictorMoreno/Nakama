using Nakama.QuestionActivity.Domain;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

public sealed class QuestionActivityEntryMapping : ClassMapping<QuestionActivityEntry>
{
    public QuestionActivityEntryMapping()
    {
        this.Table("read_question_activity");
        this.Lazy(false);

        this.Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
        });

        this.Property(x => x.BotKey);
        this.Property(x => x.UserId);
        this.Property(x => x.OccurredAt);
    }
}
