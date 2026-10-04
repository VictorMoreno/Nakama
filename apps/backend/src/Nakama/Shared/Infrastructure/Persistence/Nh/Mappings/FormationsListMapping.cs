using Nakama.FormationsList.Domain;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

public sealed class FormationsListMapping : ClassMapping<FormationSummary>
{
    public FormationsListMapping()
    {
        this.Table("read_formations_list");
        this.Lazy(false);

        this.Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
        });

        this.Property(x => x.Title);
        this.Property(x => x.Audience);
        this.Property(x => x.Description, m => m.Column(c => c.SqlType("text")));
        this.Property(x => x.Status);
        this.Property(x => x.Archived);
        this.Property(x => x.IconUrl);
        this.Property(x => x.ModulesCount);
    }
}
