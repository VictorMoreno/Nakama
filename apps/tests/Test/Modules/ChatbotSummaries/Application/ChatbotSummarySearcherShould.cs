using AwesomeAssertions;
using Nakama.ChatbotMembers.Application;
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
        this._handler = new ChatbotSummarySearcher.Handler(
            new(this.SummariesRepository.Object, this.QueryBus.Object),
            this.Storage);
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
            new ChatbotSummarySearcher.Query(Guid.NewGuid(), true),
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
            new ChatbotSummarySearcher.Query(Guid.NewGuid(), true),
            CancellationToken.None);

        response.Should().ContainSingle().Which.IconUrl.Should().Be($"{this.Storage.Value.ChatbotIconsRoute}/icon.png");
    }

    [Test]
    public async Task leave_the_icon_empty_when_the_chatbot_has_none()
    {
        this.ShouldFindAllChatbotSummaries([ChatbotSummaryMother.With(iconUrl: null)]);

        IReadOnlyCollection<ChatbotSummaryResponse> response = await this._handler.Handle(
            new ChatbotSummarySearcher.Query(Guid.NewGuid(), true),
            CancellationToken.None);

        response.Should().ContainSingle().Which.IconUrl.Should().BeNull();
    }

    [Test]
    public async Task return_an_empty_collection_when_there_are_no_chatbots()
    {
        this.ShouldFindAllChatbotSummaries([]);

        IReadOnlyCollection<ChatbotSummaryResponse> response = await this._handler.Handle(
            new ChatbotSummarySearcher.Query(Guid.NewGuid(), true),
            CancellationToken.None);

        response.Should().BeEmpty();
    }

    [Test]
    public async Task return_every_chatbot_to_an_administrator_without_looking_at_memberships()
    {
        Nakama.ChatbotSummaries.Domain.ChatbotSummary first = ChatbotSummaryMother.With();
        Nakama.ChatbotSummaries.Domain.ChatbotSummary second = ChatbotSummaryMother.With();
        this.ShouldFindAllChatbotSummaries([first, second]);

        IReadOnlyCollection<ChatbotSummaryResponse> response = await this._handler.Handle(
            new ChatbotSummarySearcher.Query(Guid.NewGuid(), true),
            CancellationToken.None);

        response.Select(item => item.Id).Should().Equal(first.Id, second.Id);
        this.ShouldNotHaveAskedForAnyMembership();
    }

    [Test]
    public async Task return_only_the_chatbots_the_user_is_a_member_of()
    {
        Guid userId = Guid.NewGuid();
        Nakama.ChatbotSummaries.Domain.ChatbotSummary mine = ChatbotSummaryMother.With();
        Nakama.ChatbotSummaries.Domain.ChatbotSummary alsoMine = ChatbotSummaryMother.With();
        this.ShouldFindMembershipsOfTheUser(
            userId,
            [new ChatbotMemberResponse(mine.Id, userId), new ChatbotMemberResponse(alsoMine.Id, userId)]);
        this.ShouldFindChatbotSummariesByIds([mine, alsoMine]);

        IReadOnlyCollection<ChatbotSummaryResponse> response = await this._handler.Handle(
            new ChatbotSummarySearcher.Query(userId, false),
            CancellationToken.None);

        response.Select(item => item.Id).Should().Equal(mine.Id, alsoMine.Id);
        this.ShouldHaveSearchedChatbotSummariesByIds([mine.Id, alsoMine.Id]);
        this.ShouldNotHaveSearchedAllChatbotSummaries();
    }

    [Test]
    public async Task return_no_chatbots_to_a_user_that_is_not_a_member_of_any()
    {
        Guid userId = Guid.NewGuid();
        this.ShouldFindMembershipsOfTheUser(userId, []);
        this.ShouldFindChatbotSummariesByIds([]);

        IReadOnlyCollection<ChatbotSummaryResponse> response = await this._handler.Handle(
            new ChatbotSummarySearcher.Query(userId, false),
            CancellationToken.None);

        response.Should().BeEmpty();
        this.ShouldHaveSearchedChatbotSummariesByIds([]);
        this.ShouldNotHaveSearchedAllChatbotSummaries();
    }
}
