using AwesomeAssertions;
using Nakama.FormationEnrollment.Application;
using Nakama.FormationEnrollment.Application.Searcher;
using Test.Modules.FormationEnrollment.Domain;
using Enrollment = Nakama.FormationEnrollment.Domain.FormationEnrollment;

namespace Test.Modules.FormationEnrollment.Application;

[TestFixture]
public sealed class UserFormationEnrollmentSearcherShould : FormationEnrollmentUnitTestCase
{
    private UserFormationEnrollmentSearcher.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new UserFormationEnrollmentSearcher.Handler(new(this.EnrollmentsRepository.Object));
    }

    [Test]
    public async Task return_the_progress_of_every_formation_the_person_is_assigned_to()
    {
        Guid userId = Guid.NewGuid();
        Guid module = Guid.NewGuid();
        Enrollment first = FormationEnrollmentMother.With(userId: userId, progress: LearnerProgressMother.With(completedModuleIds: [module]));
        Enrollment second = FormationEnrollmentMother.With(userId: userId);
        this.ShouldFindEnrollmentsOfUser(userId, first, second);

        IReadOnlyCollection<FormationEnrollmentResponse> response = await this._handler.Handle(
            new UserFormationEnrollmentSearcher.Query(userId),
            CancellationToken.None);

        response.Select(item => item.FormationId).Should().Equal(first.FormationId, second.FormationId);
        response.First().CompletedModuleIds.Should().Equal(module);
    }

    [Test]
    public async Task return_an_empty_collection_when_the_person_has_no_formations()
    {
        Guid userId = Guid.NewGuid();
        this.ShouldFindEnrollmentsOfUser(userId);

        IReadOnlyCollection<FormationEnrollmentResponse> response = await this._handler.Handle(
            new UserFormationEnrollmentSearcher.Query(userId),
            CancellationToken.None);

        response.Should().BeEmpty();
    }
}
