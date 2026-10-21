using AwesomeAssertions;
using Nakama.FormationsList.Domain;

namespace Test.Modules.FormationsList.Domain;

[TestFixture]
public sealed class FormationSummaryPageSpecificationShould
{
    [Test]
    public void exclude_archived_formations()
    {
        FormationSummaryPageSpecification specification = new(null, null, null);

        specification.IsSatisfiedBy(FormationSummaryMother.With(archived: false)).Should().BeTrue();
        specification.IsSatisfiedBy(FormationSummaryMother.With(archived: true)).Should().BeFalse();
    }

    [Test]
    public void match_the_search_text_in_the_title_audience_or_description_ignoring_case()
    {
        FormationSummaryPageSpecification specification = new(null, " Seguridad ", null);

        specification.IsSatisfiedBy(FormationSummaryMother.With(title: "Seguridad vial", audience: "x", description: "x")).Should().BeTrue();
        specification.IsSatisfiedBy(FormationSummaryMother.With(title: "x", audience: "Equipo de SEGURIDAD", description: "x")).Should().BeTrue();
        specification.IsSatisfiedBy(FormationSummaryMother.With(title: "x", audience: "x", description: "Curso de seguridad")).Should().BeTrue();
        specification.IsSatisfiedBy(FormationSummaryMother.With(title: "x", audience: "x", description: "x")).Should().BeFalse();
    }

    [Test]
    public void ignore_a_blank_search_text()
    {
        FormationSummaryPageSpecification specification = new(null, "  ", null);

        specification.Search.Should().BeNull();
        specification.IsSatisfiedBy(FormationSummaryMother.Random()).Should().BeTrue();
    }

    [Test]
    public void match_only_the_requested_status()
    {
        FormationSummaryPageSpecification specification = new(null, null, "Draft");

        specification.IsSatisfiedBy(FormationSummaryMother.With(status: "Draft")).Should().BeTrue();
        specification.IsSatisfiedBy(FormationSummaryMother.With(status: "Active")).Should().BeFalse();
    }

    [Test]
    public void match_only_the_given_ids()
    {
        FormationSummary included = FormationSummaryMother.Random();
        FormationSummaryPageSpecification specification = new([included.Id], null, null);

        specification.IsSatisfiedBy(included).Should().BeTrue();
        specification.IsSatisfiedBy(FormationSummaryMother.Random()).Should().BeFalse();
    }

    [Test]
    public void match_nothing_when_the_given_ids_are_empty()
    {
        FormationSummaryPageSpecification specification = new([], null, null);

        specification.IsSatisfiedBy(FormationSummaryMother.Random()).Should().BeFalse();
    }

    [Test]
    public void enable_paging_only_when_a_page_size_is_given()
    {
        new FormationSummaryPageSpecification(null, null, null).IsPagingEnabled.Should().BeFalse();

        FormationSummaryPageSpecification paged = new(null, null, null, skip: 12, take: 12);

        paged.IsPagingEnabled.Should().BeTrue();
        paged.Skip.Should().Be(12);
        paged.Take.Should().Be(12);
    }
}
