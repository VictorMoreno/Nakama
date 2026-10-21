using AwesomeAssertions;
using Nakama.FormationsList.Application;
using Nakama.FormationsList.Application.PageSearcher;
using Nakama.FormationsList.Domain;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Test.Modules.FormationEnrollment.Domain;
using Test.Modules.FormationsList.Domain;
using Enrollment = Nakama.FormationEnrollment.Domain.FormationEnrollment;

namespace Test.Modules.FormationsList.Application;

[TestFixture]
public sealed class FormationSummaryPageSearcherShould : FormationsListUnitTestCase
{
    private FormationSummaryPageSearcher.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationSummaryPageSearcher.Handler(
            new(this.SummariesRepository.Object, this.EnrollmentsRepository.Object, this.Storage),
            this.Pagination);
    }

    [Test]
    public async Task return_the_requested_page_with_the_total_of_matching_formations()
    {
        FormationSummary first = FormationSummaryMother.Random();
        FormationSummary second = FormationSummaryMother.Random();
        FormationSummaryPageSearcher.Query query = FormationSummaryPageSearcherQueryMother.Admin() with { Page = 2, PageSize = 5 };
        this.ShouldFindEnrollmentsOfTheUser(query.UserId, []);
        this.ShouldFindFormationSummariesPage([first, second], totalCount: 8);

        PagedResponse<FormationSummaryResponse> response = await this._handler.Handle(query, CancellationToken.None);

        response.Items.Select(item => item.Id).Should().Equal(first.Id, second.Id);
        response.TotalCount.Should().Be(8);
        response.Page.Should().Be(2);
        response.PageSize.Should().Be(5);
        response.TotalPages.Should().Be(2);
        this.ShouldHaveSearchedFormationSummariesPage(skip: 5, take: 5, ids: null);
    }

    [Test]
    public async Task use_the_default_page_size_when_none_is_requested()
    {
        FormationSummaryPageSearcher.Query query = FormationSummaryPageSearcherQueryMother.Admin();
        this.ShouldFindEnrollmentsOfTheUser(query.UserId, []);
        this.ShouldFindFormationSummariesPage([], totalCount: 0);

        PagedResponse<FormationSummaryResponse> response = await this._handler.Handle(query, CancellationToken.None);

        response.PageSize.Should().Be(this.Pagination.Value.DefaultPageSize);
        this.ShouldHaveSearchedFormationSummariesPage(skip: 0, take: this.Pagination.Value.DefaultPageSize, ids: null);
    }

    [Test]
    public async Task search_every_formation_for_an_administrator()
    {
        FormationSummaryPageSearcher.Query query = FormationSummaryPageSearcherQueryMother.Admin();
        this.ShouldFindEnrollmentsOfTheUser(query.UserId, []);
        this.ShouldFindFormationSummariesPage([], totalCount: 0);

        await this._handler.Handle(query, CancellationToken.None);

        this.ShouldHaveSearchedFormationSummariesPage(skip: 0, take: this.Pagination.Value.DefaultPageSize, ids: null);
    }

    [Test]
    public async Task restrict_the_page_to_the_enrolled_formations_of_a_regular_user()
    {
        FormationSummary enrolled = FormationSummaryMother.Random();
        Enrollment enrollment = FormationEnrollmentMother.With(formationId: enrolled.Id);
        this.ShouldFindEnrollmentsOfTheUser(enrollment.UserId, [enrollment]);
        this.ShouldFindFormationSummariesPage([enrolled], totalCount: 1);

        PagedResponse<FormationSummaryResponse> response = await this._handler.Handle(
            FormationSummaryPageSearcherQueryMother.With(enrollment.UserId),
            CancellationToken.None);

        response.Items.Should().ContainSingle().Which.IsEnrolled.Should().BeTrue();
        this.ShouldHaveSearchedFormationSummariesPage(skip: 0, take: this.Pagination.Value.DefaultPageSize, ids: [enrolled.Id]);
    }

    [Test]
    public async Task return_an_empty_page_to_a_regular_user_without_enrollments()
    {
        FormationSummaryPageSearcher.Query query = FormationSummaryPageSearcherQueryMother.Random();
        this.ShouldFindEnrollmentsOfTheUser(query.UserId, []);
        this.ShouldFindFormationSummariesPage([], totalCount: 0);

        PagedResponse<FormationSummaryResponse> response = await this._handler.Handle(query, CancellationToken.None);

        response.Items.Should().BeEmpty();
        this.ShouldHaveSearchedFormationSummariesPage(skip: 0, take: this.Pagination.Value.DefaultPageSize, ids: []);
    }

    [Test]
    public async Task calculate_the_progress_of_the_formations_in_the_page()
    {
        FormationSummary formation = FormationSummaryMother.With(modulesCount: 4);
        Enrollment enrollment = FormationEnrollmentMother.With(formationId: formation.Id);
        this.ShouldFindEnrollmentsOfTheUser(enrollment.UserId, [enrollment]);
        this.ShouldFindFormationSummariesPage([formation], totalCount: 1);

        PagedResponse<FormationSummaryResponse> response = await this._handler.Handle(
            FormationSummaryPageSearcherQueryMother.With(enrollment.UserId),
            CancellationToken.None);

        FormationSummaryResponse item = response.Items.Should().ContainSingle().Subject;
        item.CompletedModulesCount.Should().Be(enrollment.Progress.CompletedModuleIds.Count);
        item.ProgressPct.Should().Be(enrollment.Progress.CompletedModuleIds.Count * 100 / 4);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public async Task reject_a_page_number_lower_than_one(int page)
    {
        Func<Task> act = async () =>
            await this._handler.Handle(FormationSummaryPageSearcherQueryMother.Admin() with { Page = page }, CancellationToken.None);

        await act.Should().ThrowAsync<PaginationInvalidPageError>();
        this.ShouldNotHaveSearchedFormationSummariesPage();
    }

    [TestCase(0)]
    [TestCase(-5)]
    [TestCase(51)]
    public async Task reject_a_page_size_outside_the_allowed_range(int pageSize)
    {
        Func<Task> act = async () =>
            await this._handler.Handle(FormationSummaryPageSearcherQueryMother.Admin() with { PageSize = pageSize }, CancellationToken.None);

        await act.Should().ThrowAsync<PaginationInvalidPageSizeError>();
        this.ShouldNotHaveSearchedFormationSummariesPage();
    }
}
