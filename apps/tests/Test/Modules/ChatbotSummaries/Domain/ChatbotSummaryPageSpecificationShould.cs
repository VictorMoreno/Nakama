using AwesomeAssertions;
using Nakama.ChatbotSummaries.Domain;

namespace Test.Modules.ChatbotSummaries.Domain;

[TestFixture]
public sealed class ChatbotSummaryPageSpecificationShould
{
    [Test]
    public void exclude_archived_chatbots()
    {
        ChatbotSummaryPageSpecification specification = new(null, null, null);

        specification.IsSatisfiedBy(ChatbotSummaryMother.With(archived: false)).Should().BeTrue();
        specification.IsSatisfiedBy(ChatbotSummaryMother.With(archived: true)).Should().BeFalse();
    }

    [Test]
    public void match_the_search_text_in_the_label_ignoring_case()
    {
        ChatbotSummaryPageSpecification specification = new(null, "  ONBoarding ", null);

        specification.IsSatisfiedBy(ChatbotSummaryMother.With(label: "Asistente de onboarding")).Should().BeTrue();
        specification.IsSatisfiedBy(ChatbotSummaryMother.With(label: "Soporte", description: "nada")).Should().BeFalse();
    }

    [Test]
    public void match_the_search_text_in_the_description_ignoring_case()
    {
        ChatbotSummaryPageSpecification specification = new(null, "vacaciones", null);

        specification.IsSatisfiedBy(ChatbotSummaryMother.With(label: "RRHH", description: "Política de VACACIONES")).Should().BeTrue();
    }

    [Test]
    public void ignore_a_blank_search_text()
    {
        ChatbotSummaryPageSpecification specification = new(null, "   ", null);

        specification.Search.Should().BeNull();
        specification.IsSatisfiedBy(ChatbotSummaryMother.With()).Should().BeTrue();
    }

    [Test]
    public void match_only_the_requested_status()
    {
        ChatbotSummaryPageSpecification specification = new(null, null, "Draft");

        specification.IsSatisfiedBy(ChatbotSummaryMother.With(status: "Draft")).Should().BeTrue();
        specification.IsSatisfiedBy(ChatbotSummaryMother.With(status: "Active")).Should().BeFalse();
    }

    [Test]
    public void match_only_the_given_ids()
    {
        ChatbotSummary included = ChatbotSummaryMother.With();
        ChatbotSummaryPageSpecification specification = new([included.Id], null, null);

        specification.IsSatisfiedBy(included).Should().BeTrue();
        specification.IsSatisfiedBy(ChatbotSummaryMother.With()).Should().BeFalse();
    }

    [Test]
    public void match_nothing_when_the_given_ids_are_empty()
    {
        ChatbotSummaryPageSpecification specification = new([], null, null);

        specification.IsSatisfiedBy(ChatbotSummaryMother.With()).Should().BeFalse();
    }

    [Test]
    public void combine_every_filter()
    {
        ChatbotSummary match = ChatbotSummaryMother.With(label: "Soporte técnico", status: "Active");
        ChatbotSummaryPageSpecification specification = new([match.Id], "soporte", "Active");

        specification.IsSatisfiedBy(match).Should().BeTrue();
        specification.IsSatisfiedBy(ChatbotSummaryMother.With(id: match.Id, label: "Soporte técnico", status: "Draft")).Should().BeFalse();
    }

    [Test]
    public void enable_paging_only_when_a_page_size_is_given()
    {
        new ChatbotSummaryPageSpecification(null, null, null).IsPagingEnabled.Should().BeFalse();

        ChatbotSummaryPageSpecification paged = new(null, null, null, skip: 20, take: 10);

        paged.IsPagingEnabled.Should().BeTrue();
        paged.Skip.Should().Be(20);
        paged.Take.Should().Be(10);
    }
}
