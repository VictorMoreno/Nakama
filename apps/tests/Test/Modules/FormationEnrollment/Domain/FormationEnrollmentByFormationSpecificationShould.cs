using AwesomeAssertions;
using Nakama.FormationEnrollment.Domain;

namespace Test.Modules.FormationEnrollment.Domain;

[TestFixture]
public sealed class FormationEnrollmentByFormationSpecificationShould
{
    [Test]
    public void match_only_the_enrollments_of_that_formation()
    {
        Guid formationId = Guid.NewGuid();
        FormationEnrollmentByFormationSpecification specification = new(formationId);

        specification.IsSatisfiedBy(FormationEnrollmentMother.With(formationId: formationId)).Should().BeTrue();
        specification.IsSatisfiedBy(FormationEnrollmentMother.With(formationId: Guid.NewGuid())).Should().BeFalse();
    }
}
