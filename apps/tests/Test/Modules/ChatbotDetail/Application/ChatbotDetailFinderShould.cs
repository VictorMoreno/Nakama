using AwesomeAssertions;
using Nakama.ChatbotDetail.Application;
using Nakama.ChatbotDetail.Application.Finder;
using Nakama.ChatbotDetail.Domain;
using Nakama.ChatbotDetail.Domain.Errors;
using Test.Modules.ChatbotDetail.Domain;
using ChatbotDetailEntity = Nakama.ChatbotDetail.Domain.ChatbotDetail;

namespace Test.Modules.ChatbotDetail.Application;

[TestFixture]
public sealed class ChatbotDetailFinderShould : ChatbotDetailUnitTestCase
{
    private ChatbotDetailFinder.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotDetailFinder.Handler(new(this.DetailRepository.Object), this.Storage);
    }

    [Test]
    public async Task return_the_detail_as_a_response_with_its_documents_team_gaps_questions_and_stats()
    {
        ChatbotDetailEntity detail = ChatbotDetailMother.Populated();
        this.ShouldFindChatbotDetail(detail);

        ChatbotDetailResponse response = await this._handler.Handle(
            ChatbotDetailFinderQueryMother.With(detail.Id),
            CancellationToken.None);

        response.Should().BeEquivalentTo(detail);
    }

    [Test]
    public async Task prefix_the_icon_with_the_icons_route_of_the_chatbots()
    {
        ChatbotDetailEntity detail = ChatbotDetailMother.With(iconUrl: "icon.png");
        this.ShouldFindChatbotDetail(detail);

        ChatbotDetailResponse response = await this._handler.Handle(
            ChatbotDetailFinderQueryMother.With(detail.Id),
            CancellationToken.None);

        response.IconUrl.Should().Be($"{this.Storage.Value.ChatbotIconsRoute}/icon.png");
    }

    [Test]
    public async Task leave_the_icon_empty_when_the_chatbot_has_none()
    {
        ChatbotDetailEntity detail = ChatbotDetailMother.With(iconUrl: null);
        this.ShouldFindChatbotDetail(detail);

        ChatbotDetailResponse response = await this._handler.Handle(
            ChatbotDetailFinderQueryMother.With(detail.Id),
            CancellationToken.None);

        response.IconUrl.Should().BeNull();
    }

    [Test]
    public async Task return_empty_lists_for_a_chatbot_that_has_just_been_created()
    {
        ChatbotDetailEntity detail = ChatbotDetailMother.Random();
        this.ShouldFindChatbotDetail(detail);

        ChatbotDetailResponse response = await this._handler.Handle(
            ChatbotDetailFinderQueryMother.With(detail.Id),
            CancellationToken.None);

        response.Docs.Should().BeEmpty();
        response.Team.Should().BeEmpty();
        response.Gaps.Should().BeEmpty();
        response.TopQuestions.Should().BeEmpty();
        response.Stats.Should().BeEmpty();
    }

    [Test]
    public async Task fail_when_the_chatbot_detail_does_not_exist()
    {
        this.ShouldFindChatbotDetail(null);

        Func<Task> act = async () =>
            await this._handler.Handle(ChatbotDetailFinderQueryMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<ChatbotDetailNotFoundError>();
    }
}
