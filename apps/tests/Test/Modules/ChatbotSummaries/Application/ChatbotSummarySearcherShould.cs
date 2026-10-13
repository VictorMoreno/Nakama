using AwesomeAssertions;
using Nakama.ChatbotSummaries.Application;
using Nakama.ChatbotSummaries.Application.Searcher;
using Test.Modules.ChatbotSummaries.Domain;

namespace Test.Modules.ChatbotSummaries.Application;

[TestFixture]
public sealed class ChatbotSummarySearcherShould : ChatbotSummariesUnitTestCase
{
    private ChatbotSummarySearcher.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotSummarySearcher.Handler(new(this.SummariesRepository.Object), this.Storage);
    }

    [Test]
    public async Task return_a_response_per_chatbot_with_its_counters_and_archived_flag()
    {
        Nakama.ChatbotSummaries.Domain.ChatbotSummary active = ChatbotSummaryMother.With(
            docsCount: 3,
            teamCount: 2,
            archived: false);
        Nakama.ChatbotSummaries.Domain.ChatbotSummary archived = ChatbotSummaryMother.With(
            docsCount: 0,
            teamCount: 1,
            archived: true);
        this.ShouldFindAllChatbotSummaries([active, archived]);

        IReadOnlyCollection<ChatbotSummaryResponse> response = await this._handler.Handle(
            new ChatbotSummarySearcher.Query(),
            CancellationToken.None);

        response.Select(item => (item.Id, item.Label, item.Description, item.Status, item.DocsCount, item.TeamCount, item.Archived))
            .Should().Equal(
                (active.Id, active.Label, active.Description, active.Status, 3, 2, false),
                (archived.Id, archived.Label, archived.Description, archived.Status, 0, 1, true));
    }

    [Test]
    public async Task prefix_the_icon_with_the_icons_route_of_the_chatbots()
    {
        this.ShouldFindAllChatbotSummaries([ChatbotSummaryMother.With(iconUrl: "icon.png")]);

        IReadOnlyCollection<ChatbotSummaryResponse> response = await this._handler.Handle(
            new ChatbotSummarySearcher.Query(),
            CancellationToken.None);

        response.Should().ContainSingle().Which.IconUrl.Should().Be($"{this.Storage.Value.ChatbotIconsRoute}/icon.png");
    }

    [Test]
    public async Task leave_the_icon_empty_when_the_chatbot_has_none()
    {
        this.ShouldFindAllChatbotSummaries([ChatbotSummaryMother.With(iconUrl: null)]);

        IReadOnlyCollection<ChatbotSummaryResponse> response = await this._handler.Handle(
            new ChatbotSummarySearcher.Query(),
            CancellationToken.None);

        response.Should().ContainSingle().Which.IconUrl.Should().BeNull();
    }

    [Test]
    public async Task return_an_empty_collection_when_there_are_no_chatbots()
    {
        this.ShouldFindAllChatbotSummaries([]);

        IReadOnlyCollection<ChatbotSummaryResponse> response = await this._handler.Handle(
            new ChatbotSummarySearcher.Query(),
            CancellationToken.None);

        response.Should().BeEmpty();
    }
}
