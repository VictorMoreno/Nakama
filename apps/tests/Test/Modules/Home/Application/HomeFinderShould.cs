using AwesomeAssertions;
using Nakama.FormationDetail.Application;
using Nakama.FormationEnrollment.Application;
using Nakama.Home.Application;
using Nakama.Home.Application.Finder;
using Test.Modules.FormationDetail.Application;

namespace Test.Modules.Home.Application;

[TestFixture]
public sealed class HomeFinderShould : HomeUnitTestCase
{
    private HomeFinder.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new HomeFinder.Handler(new(this.QueryBus.Object));
        this.ShouldFindQuestionsOfTheCurrentWeek(0);
        this.ShouldFindActiveDays(0);
        this.ShouldFindEnrollmentsOfTheUser();
    }

    [Test]
    public async Task return_the_questions_of_this_week_and_the_active_days_of_the_person()
    {
        Guid userId = Guid.NewGuid();
        this.ShouldFindQuestionsOfTheCurrentWeek(12);
        this.ShouldFindActiveDays(5);

        HomeResponse response = await this._handler.Handle(new HomeFinder.Query(userId), CancellationToken.None);

        response.QuestionsThisWeek.Should().Be(12);
        response.ActiveDaysLast30.Should().Be(5);
        response.Formations.Should().BeEmpty();
        response.PendingExams.Should().BeEmpty();
        this.ShouldHaveAskedForTheQuestionsOf(userId);
        this.ShouldHaveAskedForTheActivityOf(userId);
        this.ShouldHaveAskedForTheEnrollmentsOf(userId);
    }

    [Test]
    public async Task return_the_progress_and_the_next_module_of_each_active_formation()
    {
        FormationDetailResponse detail = FormationDetailResponseMother.With(
            modules:
            [
                FormationModuleResponseMother.Random(),
                FormationModuleResponseMother.Random(),
                FormationModuleResponseMother.Random(),
                FormationModuleResponseMother.Random()
            ]);
        FormationModuleResponse next = detail.Modules[1];
        this.ShouldFindFormationDetail(detail);
        this.ShouldFindEnrollmentsOfTheUser(Enrollment(detail.Id, completed: [detail.Modules[0].Id]));

        HomeResponse response = await this._handler.Handle(new HomeFinder.Query(Guid.NewGuid()), CancellationToken.None);

        HomeFormationRow row = response.Formations.Should().ContainSingle().Subject;
        row.FormationId.Should().Be(detail.Id);
        row.Title.Should().Be(detail.Title);
        row.CompletedModules.Should().Be(1);
        row.ModulesCount.Should().Be(4);
        row.ProgressPct.Should().Be(25);
        row.NextModuleTitle.Should().Be(next.Title);
        row.Completed.Should().BeFalse();
    }

    [Test]
    public async Task mark_a_formation_as_completed_when_every_module_is_done()
    {
        FormationDetailResponse detail = FormationDetailResponseMother.WithTwoModules(firstGated: false);
        this.ShouldFindFormationDetail(detail);
        this.ShouldFindEnrollmentsOfTheUser(Enrollment(detail.Id, completed: detail.Modules.Select(module => module.Id).ToList()));

        HomeResponse response = await this._handler.Handle(new HomeFinder.Query(Guid.NewGuid()), CancellationToken.None);

        HomeFormationRow row = response.Formations.Should().ContainSingle().Subject;
        row.Completed.Should().BeTrue();
        row.ProgressPct.Should().Be(100);
        row.NextModuleTitle.Should().BeNull();
    }

    [Test]
    public async Task not_complete_a_formation_without_modules()
    {
        FormationDetailResponse detail = FormationDetailResponseMother.With(modules: []);
        this.ShouldFindFormationDetail(detail);
        this.ShouldFindEnrollmentsOfTheUser(Enrollment(detail.Id));

        HomeResponse response = await this._handler.Handle(new HomeFinder.Query(Guid.NewGuid()), CancellationToken.None);

        HomeFormationRow row = response.Formations.Should().ContainSingle().Subject;
        row.Completed.Should().BeFalse();
        row.ProgressPct.Should().Be(0);
    }

    [TestCase("Draft", false)]
    [TestCase("Inactive", false)]
    [TestCase("Active", true)]
    public async Task leave_out_the_formations_that_are_archived_or_not_active(string status, bool archived)
    {
        FormationDetailResponse detail = FormationDetailResponseMother.With(status: status, archived: archived, modules: [FormationModuleResponseMother.Random()]);
        this.ShouldFindFormationDetail(detail);
        this.ShouldFindEnrollmentsOfTheUser(Enrollment(detail.Id));

        HomeResponse response = await this._handler.Handle(new HomeFinder.Query(Guid.NewGuid()), CancellationToken.None);

        response.Formations.Should().BeEmpty();
        response.PendingExams.Should().BeEmpty();
    }

    [Test]
    public async Task list_the_exams_tried_and_not_passed_with_the_score_of_the_last_attempt()
    {
        FormationModuleResponse failed = FormationModuleResponseMother.Exam(70, 0);
        FormationModuleResponse passed = FormationModuleResponseMother.Exam(50, 0);
        FormationModuleResponse untouched = FormationModuleResponseMother.Exam(80, 0);
        FormationDetailResponse detail = FormationDetailResponseMother.With(modules: [failed, passed, untouched]);
        this.ShouldFindFormationDetail(detail);
        this.ShouldFindEnrollmentsOfTheUser(Enrollment(
            detail.Id,
            completed: [passed.Id],
            attempts:
            [
                new ExamAttemptResponse(failed.Id, 20, false, DateTime.UtcNow.AddHours(-2)),
                new ExamAttemptResponse(failed.Id, 55, false, DateTime.UtcNow.AddHours(-1)),
                new ExamAttemptResponse(passed.Id, 90, true, DateTime.UtcNow)
            ]));

        HomeResponse response = await this._handler.Handle(new HomeFinder.Query(Guid.NewGuid()), CancellationToken.None);

        HomePendingExamRow row = response.PendingExams.Should().ContainSingle().Subject;
        row.FormationId.Should().Be(detail.Id);
        row.FormationTitle.Should().Be(detail.Title);
        row.ModuleId.Should().Be(failed.Id);
        row.ModuleTitle.Should().Be(failed.Title);
        row.LastScore.Should().Be(55);
        row.PassingScore.Should().Be(70);
    }

    private static FormationEnrollmentResponse Enrollment(
        Guid formationId,
        IReadOnlyList<Guid>? completed = null,
        IReadOnlyList<ExamAttemptResponse>? attempts = null) =>
        new(formationId, Guid.NewGuid(), completed ?? [], new Dictionary<Guid, int>(), attempts ?? []);
}
