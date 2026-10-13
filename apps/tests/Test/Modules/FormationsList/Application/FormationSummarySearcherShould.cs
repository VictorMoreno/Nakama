using AwesomeAssertions;
using Nakama.FormationsList.Application;
using Nakama.FormationsList.Application.Searcher;
using Nakama.FormationsList.Domain;
using Test.Modules.FormationEnrollment.Domain;
using Test.Modules.FormationsList.Domain;
using Enrollment = Nakama.FormationEnrollment.Domain.FormationEnrollment;

namespace Test.Modules.FormationsList.Application;

[TestFixture]
public sealed class FormationSummarySearcherShould : FormationsListUnitTestCase
{
    private FormationSummarySearcher.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationSummarySearcher.Handler(
            new(this.SummariesRepository.Object, this.EnrollmentsRepository.Object, this.Storage));
    }

    [Test]
    public async Task show_every_formation_to_an_administrator_without_looking_at_the_enrollments()
    {
        FormationSummary first = FormationSummaryMother.Random();
        FormationSummary second = FormationSummaryMother.Random();
        this.ShouldFindEnrollmentsOfTheUser([]);
        this.ShouldFindAllFormationSummaries([first, second]);

        IReadOnlyCollection<FormationSummaryResponse> response = await this._handler.Handle(
            FormationSummarySearcherQueryMother.Admin(),
            CancellationToken.None);

        response.Select(item => item.Id).Should().Equal(first.Id, second.Id);
        this.ShouldHaveSearchedAllFormationSummaries();
        this.ShouldNotHaveSearchedFormationSummariesByIds();
    }

    [Test]
    public async Task show_only_the_enrolled_formations_to_a_regular_user()
    {
        FormationSummary enrolled = FormationSummaryMother.Random();
        Enrollment enrollment = FormationEnrollmentMother.With(formationId: enrolled.Id);
        this.ShouldFindEnrollmentsOfTheUser([enrollment]);
        this.ShouldFindFormationSummariesByIds([enrolled]);

        IReadOnlyCollection<FormationSummaryResponse> response = await this._handler.Handle(
            FormationSummarySearcherQueryMother.With(enrollment.UserId),
            CancellationToken.None);

        response.Should().ContainSingle().Which.Id.Should().Be(enrolled.Id);
        this.ShouldHaveSearchedFormationSummariesByIds([enrolled.Id]);
        this.ShouldNotHaveSearchedAllFormationSummaries();
    }

    [Test]
    public async Task return_an_empty_collection_to_a_regular_user_without_enrollments()
    {
        this.ShouldFindEnrollmentsOfTheUser([]);
        this.ShouldFindFormationSummariesByIds([]);

        IReadOnlyCollection<FormationSummaryResponse> response = await this._handler.Handle(
            FormationSummarySearcherQueryMother.Random(),
            CancellationToken.None);

        response.Should().BeEmpty();
        this.ShouldHaveSearchedFormationSummariesByIds([]);
    }

    [Test]
    public async Task report_the_progress_as_the_percentage_of_completed_modules()
    {
        FormationSummary summary = FormationSummaryMother.With(modulesCount: 4);
        Enrollment enrollment = FormationEnrollmentMother.With(
            formationId: summary.Id,
            progress: LearnerProgressMother.With(completedModuleIds: [Guid.NewGuid()]));
        this.ShouldFindEnrollmentsOfTheUser([enrollment]);
        this.ShouldFindFormationSummariesByIds([summary]);

        IReadOnlyCollection<FormationSummaryResponse> response = await this._handler.Handle(
            FormationSummarySearcherQueryMother.With(enrollment.UserId),
            CancellationToken.None);

        FormationSummaryResponse item = response.Should().ContainSingle().Which;
        item.ModulesCount.Should().Be(4);
        item.CompletedModulesCount.Should().Be(1);
        item.ProgressPct.Should().Be(25);
        item.IsEnrolled.Should().BeTrue();
    }

    [Test]
    public async Task round_the_progress_down_to_a_whole_percentage()
    {
        FormationSummary summary = FormationSummaryMother.With(modulesCount: 3);
        Enrollment enrollment = FormationEnrollmentMother.With(
            formationId: summary.Id,
            progress: LearnerProgressMother.With(completedModuleIds: [Guid.NewGuid()]));
        this.ShouldFindEnrollmentsOfTheUser([enrollment]);
        this.ShouldFindFormationSummariesByIds([summary]);

        IReadOnlyCollection<FormationSummaryResponse> response = await this._handler.Handle(
            FormationSummarySearcherQueryMother.With(enrollment.UserId),
            CancellationToken.None);

        response.Should().ContainSingle().Which.ProgressPct.Should().Be(33);
    }

    [Test]
    public async Task report_a_complete_progress_when_every_module_is_completed()
    {
        FormationSummary summary = FormationSummaryMother.With(modulesCount: 2);
        Enrollment enrollment = FormationEnrollmentMother.With(
            formationId: summary.Id,
            progress: LearnerProgressMother.With(completedModuleIds: [Guid.NewGuid(), Guid.NewGuid()]));
        this.ShouldFindEnrollmentsOfTheUser([enrollment]);
        this.ShouldFindFormationSummariesByIds([summary]);

        IReadOnlyCollection<FormationSummaryResponse> response = await this._handler.Handle(
            FormationSummarySearcherQueryMother.With(enrollment.UserId),
            CancellationToken.None);

        response.Should().ContainSingle().Which.ProgressPct.Should().Be(100);
    }

    [Test]
    public async Task report_zero_progress_when_the_formation_has_no_modules()
    {
        FormationSummary summary = FormationSummaryMother.With(modulesCount: 0);
        Enrollment enrollment = FormationEnrollmentMother.With(formationId: summary.Id);
        this.ShouldFindEnrollmentsOfTheUser([enrollment]);
        this.ShouldFindFormationSummariesByIds([summary]);

        IReadOnlyCollection<FormationSummaryResponse> response = await this._handler.Handle(
            FormationSummarySearcherQueryMother.With(enrollment.UserId),
            CancellationToken.None);

        response.Should().ContainSingle().Which.ProgressPct.Should().Be(0);
    }

    [Test]
    public async Task mark_the_formation_as_not_enrolled_with_zero_progress_for_an_administrator_who_is_not_enrolled()
    {
        FormationSummary summary = FormationSummaryMother.With(modulesCount: 5);
        this.ShouldFindEnrollmentsOfTheUser([]);
        this.ShouldFindAllFormationSummaries([summary]);

        IReadOnlyCollection<FormationSummaryResponse> response = await this._handler.Handle(
            FormationSummarySearcherQueryMother.Admin(),
            CancellationToken.None);

        FormationSummaryResponse item = response.Should().ContainSingle().Which;
        item.IsEnrolled.Should().BeFalse();
        item.CompletedModulesCount.Should().Be(0);
        item.ProgressPct.Should().Be(0);
    }

    [Test]
    public async Task compose_the_icon_url_with_the_icons_route_of_the_formations()
    {
        FormationSummary withIcon = FormationSummaryMother.With(iconUrl: "icon.png");
        FormationSummary withoutIcon = FormationSummaryMother.With(iconUrl: null);
        this.ShouldFindEnrollmentsOfTheUser([]);
        this.ShouldFindAllFormationSummaries([withIcon, withoutIcon]);

        IReadOnlyCollection<FormationSummaryResponse> response = await this._handler.Handle(
            FormationSummarySearcherQueryMother.Admin(),
            CancellationToken.None);

        response.Select(item => item.IconUrl).Should().Equal(
            $"{this.Storage.Value.FormationIconsRoute}/icon.png",
            null);
    }
}
