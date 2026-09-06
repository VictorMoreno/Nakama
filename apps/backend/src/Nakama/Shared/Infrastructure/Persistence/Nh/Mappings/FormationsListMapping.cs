using Nakama.FormationsList.Domain;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

public sealed class FormationsListMapping : ClassMapping<FormationSummary>
{
    public FormationsListMapping()
    {
        Table("read_formations_list");

        Id(x => x.Key, m => m.Generator(Generators.Assigned));

        Property(x => x.Title);
        Property(x => x.Audience);
        Property(x => x.Description);
        Property(x => x.Status);
        Property(x => x.Archived);
        Property(x => x.IconUrl);
        Property(x => x.ProgressPct);
        Property(x => x.ModulesCount);
        Property(x => x.CompletedModulesCount);
    }
}
