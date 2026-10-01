using Nakama.FormationEnrollment.Domain;
using Nakama.Shared.Infrastructure.Persistence.Nh.UserTypes;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.Mappings;

using FormationEnrollment = Nakama.FormationEnrollment.Domain.FormationEnrollment;

public sealed class FormationEnrollmentMapping : ClassMapping<FormationEnrollment>
{
    public FormationEnrollmentMapping()
    {
        Table("formation_enrollments");
        Lazy(false);

        Id(x => x.Id, m =>
        {
            m.Column("id");
            m.Generator(Generators.Assigned);
            m.Type<GuidValueObjectUserType<FormationEnrollmentId>>();
        });

        Property(x => x.FormationId, m => m.Column("formation_id"));
        Property(x => x.UserId, m => m.Column("user_id"));
        Property(x => x.Progress, m =>
        {
            m.Type<JsonUserType<LearnerProgress>>();
            m.Column(c => { c.Name("progress_json"); c.SqlType("text"); });
        });
    }
}
