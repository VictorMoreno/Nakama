using AwesomeAssertions;
using Nakama.FormationDetail.Application;
using Nakama.FormationDetail.Application.Finder;
using Nakama.FormationDetail.Domain.Errors;
using Test.Modules.FormationDetail.Domain;
using FormationDetailEntity = Nakama.FormationDetail.Domain.FormationDetail;

namespace Test.Modules.FormationDetail.Application;

[TestFixture]
public sealed class FormationDetailFinderShould : FormationDetailUnitTestCase
{
    private FormationDetailFinder.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationDetailFinder.Handler(new(this.DetailRepository.Object), this.Storage);
    }

    [Test]
    public async Task return_the_detail_as_a_response_with_its_modules_and_team()
    {
        FormationDetailEntity detail = FormationDetailMother.Populated();
        this.ShouldFindFormationDetail(detail);

        FormationDetailResponse response = await this._handler.Handle(
            FormationDetailFinderQueryMother.With(detail.Id),
            CancellationToken.None);

        response.Should().BeEquivalentTo(detail);
    }

    [Test]
    public async Task return_the_archived_flag_and_the_linked_chatbot()
    {
        Guid chatbotKey = Guid.NewGuid();
        FormationDetailEntity detail = FormationDetailMother.With(
            archived: true,
            linkedChatbotKey: chatbotKey,
            linkedChatbotLabel: "Soporte");
        this.ShouldFindFormationDetail(detail);

        FormationDetailResponse response = await this._handler.Handle(
            FormationDetailFinderQueryMother.With(detail.Id),
            CancellationToken.None);

        response.Archived.Should().BeTrue();
        response.LinkedChatbotKey.Should().Be(chatbotKey);
        response.LinkedChatbotLabel.Should().Be("Soporte");
    }

    [Test]
    public async Task prefix_the_icon_with_the_icons_route_of_the_formations()
    {
        FormationDetailEntity detail = FormationDetailMother.With(iconUrl: "icon.png");
        this.ShouldFindFormationDetail(detail);

        FormationDetailResponse response = await this._handler.Handle(
            FormationDetailFinderQueryMother.With(detail.Id),
            CancellationToken.None);

        response.IconUrl.Should().Be($"{this.Storage.Value.FormationIconsRoute}/icon.png");
        response.IconUrl.Should().NotStartWith(this.Storage.Value.ChatbotIconsRoute);
    }

    [Test]
    public async Task leave_the_icon_empty_when_the_formation_has_none()
    {
        FormationDetailEntity detail = FormationDetailMother.With(iconUrl: null);
        this.ShouldFindFormationDetail(detail);

        FormationDetailResponse response = await this._handler.Handle(
            FormationDetailFinderQueryMother.With(detail.Id),
            CancellationToken.None);

        response.IconUrl.Should().BeNull();
    }

    [Test]
    public async Task return_no_linked_chatbot_when_the_formation_has_none()
    {
        FormationDetailEntity detail = FormationDetailMother.Random();
        this.ShouldFindFormationDetail(detail);

        FormationDetailResponse response = await this._handler.Handle(
            FormationDetailFinderQueryMother.With(detail.Id),
            CancellationToken.None);

        response.LinkedChatbotKey.Should().BeNull();
        response.LinkedChatbotLabel.Should().BeNull();
    }

    [Test]
    public async Task fail_when_the_formation_detail_does_not_exist()
    {
        this.ShouldFindFormationDetail(null);

        Func<Task> act = async () =>
            await this._handler.Handle(FormationDetailFinderQueryMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<FormationDetailNotFoundError>();
    }
}
