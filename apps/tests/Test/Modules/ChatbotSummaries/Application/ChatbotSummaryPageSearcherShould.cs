using AwesomeAssertions;
using Nakama.ChatbotMembers.Application;
using Nakama.ChatbotSummaries.Application;
using Nakama.ChatbotSummaries.Application.PageSearcher;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Test.Modules.ChatbotSummaries.Domain;

namespace Test.Modules.ChatbotSummaries.Application;

[TestFixture]
public sealed class ChatbotSummaryPageSearcherShould : ChatbotSummariesUnitTestCase
{
    private ChatbotSummaryPageSearcher.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotSummaryPageSearcher.Handler(
            new(this.SummariesRepository.Object, this.QueryBus.Object),
            this.Storage,
            this.Pagination);
    }

    [Test]
    public async Task return_the_requested_page_with_the_total_of_matching_chatbots()
    {
        Nakama.ChatbotSummaries.Domain.ChatbotSummary first = ChatbotSummaryMother.With();
        Nakama.ChatbotSummaries.Domain.ChatbotSummary second = ChatbotSummaryMother.With();
        this.ShouldFindChatbotSummariesPage([first, second], totalCount: 27);

        PagedResponse<ChatbotSummaryResponse> response = await this._handler.Handle(
            ChatbotSummaryPageSearcherQueryMother.Admin() with { Page = 3, PageSize = 10 },
            CancellationToken.None);

        response.Items.Select(item => item.Id).Should().Equal(first.Id, second.Id);
        response.TotalCount.Should().Be(27);
        response.Page.Should().Be(3);
        response.PageSize.Should().Be(10);
        response.TotalPages.Should().Be(3);
        this.ShouldHaveSearchedChatbotSummariesPage(skip: 20, take: 10, ids: null);
    }

    [Test]
    public async Task use_the_default_page_size_when_none_is_requested()
    {
        this.ShouldFindChatbotSummariesPage([], totalCount: 0);

        PagedResponse<ChatbotSummaryResponse> response = await this._handler.Handle(
            ChatbotSummaryPageSearcherQueryMother.Admin(),
            CancellationToken.None);

        response.PageSize.Should().Be(this.Pagination.Value.DefaultPageSize);
        this.ShouldHaveSearchedChatbotSummariesPage(skip: 0, take: this.Pagination.Value.DefaultPageSize, ids: null);
    }

    [Test]
    public async Task return_an_empty_page_when_there_are_no_chatbots()
    {
        this.ShouldFindChatbotSummariesPage([], totalCount: 0);

        PagedResponse<ChatbotSummaryResponse> response = await this._handler.Handle(
            ChatbotSummaryPageSearcherQueryMother.Admin(),
            CancellationToken.None);

        response.Items.Should().BeEmpty();
        response.TotalCount.Should().Be(0);
    }

    [Test]
    public async Task prefix_the_icon_with_the_icons_route_of_the_chatbots()
    {
        this.ShouldFindChatbotSummariesPage([ChatbotSummaryMother.With(iconUrl: "icon.png")], totalCount: 1);

        PagedResponse<ChatbotSummaryResponse> response = await this._handler.Handle(
            ChatbotSummaryPageSearcherQueryMother.Admin(),
            CancellationToken.None);

        response.Items.Should().ContainSingle().Which.IconUrl.Should().Be($"{this.Storage.Value.ChatbotIconsRoute}/icon.png");
    }

    [Test]
    public async Task search_every_chatbot_for_an_administrator_without_looking_at_memberships()
    {
        this.ShouldFindChatbotSummariesPage([], totalCount: 0);

        await this._handler.Handle(ChatbotSummaryPageSearcherQueryMother.Admin(), CancellationToken.None);

        this.ShouldNotHaveAskedForAnyMembership();
        this.ShouldHaveSearchedChatbotSummariesPage(skip: 0, take: this.Pagination.Value.DefaultPageSize, ids: null);
    }

    [Test]
    public async Task restrict_the_page_to_the_chatbots_the_user_is_a_member_of()
    {
        Guid userId = Guid.NewGuid();
        Nakama.ChatbotSummaries.Domain.ChatbotSummary mine = ChatbotSummaryMother.With();
        Nakama.ChatbotSummaries.Domain.ChatbotSummary alsoMine = ChatbotSummaryMother.With();
        this.ShouldFindMembershipsOfTheUser(
            userId,
            [new ChatbotMemberResponse(mine.Id, userId), new ChatbotMemberResponse(alsoMine.Id, userId)]);
        this.ShouldFindChatbotSummariesPage([mine, alsoMine], totalCount: 2);

        PagedResponse<ChatbotSummaryResponse> response = await this._handler.Handle(
            ChatbotSummaryPageSearcherQueryMother.With(userId),
            CancellationToken.None);

        response.Items.Select(item => item.Id).Should().Equal(mine.Id, alsoMine.Id);
        this.ShouldHaveSearchedChatbotSummariesPage(skip: 0, take: this.Pagination.Value.DefaultPageSize, ids: [mine.Id, alsoMine.Id]);
    }

    [Test]
    public async Task return_an_empty_page_to_a_user_that_is_not_a_member_of_any_chatbot()
    {
        Guid userId = Guid.NewGuid();
        this.ShouldFindMembershipsOfTheUser(userId, []);
        this.ShouldFindChatbotSummariesPage([], totalCount: 0);

        PagedResponse<ChatbotSummaryResponse> response = await this._handler.Handle(
            ChatbotSummaryPageSearcherQueryMother.With(userId),
            CancellationToken.None);

        response.Items.Should().BeEmpty();
        this.ShouldHaveSearchedChatbotSummariesPage(skip: 0, take: this.Pagination.Value.DefaultPageSize, ids: []);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public async Task reject_a_page_number_lower_than_one(int page)
    {
        Func<Task> act = async () =>
            await this._handler.Handle(ChatbotSummaryPageSearcherQueryMother.Admin() with { Page = page }, CancellationToken.None);

        await act.Should().ThrowAsync<PaginationInvalidPageError>();
        this.ShouldNotHaveSearchedChatbotSummariesPage();
    }

    [TestCase(0)]
    [TestCase(-5)]
    [TestCase(51)]
    public async Task reject_a_page_size_outside_the_allowed_range(int pageSize)
    {
        Func<Task> act = async () =>
            await this._handler.Handle(ChatbotSummaryPageSearcherQueryMother.Admin() with { PageSize = pageSize }, CancellationToken.None);

        await act.Should().ThrowAsync<PaginationInvalidPageSizeError>();
        this.ShouldNotHaveSearchedChatbotSummariesPage();
    }

    [Test]
    public async Task accept_a_page_size_equal_to_the_maximum()
    {
        this.ShouldFindChatbotSummariesPage([], totalCount: 0);

        PagedResponse<ChatbotSummaryResponse> response = await this._handler.Handle(
            ChatbotSummaryPageSearcherQueryMother.Admin() with { PageSize = this.Pagination.Value.MaxPageSize },
            CancellationToken.None);

        response.PageSize.Should().Be(this.Pagination.Value.MaxPageSize);
    }
}
