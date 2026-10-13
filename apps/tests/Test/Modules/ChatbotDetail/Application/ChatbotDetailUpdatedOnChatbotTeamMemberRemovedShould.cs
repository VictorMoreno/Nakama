using AwesomeAssertions;
using Nakama.ChatbotDetail.Application.Handlers;
using Nakama.ChatbotDetail.Domain;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Events;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.ChatbotDetail.Domain;
using Test.Modules.Chatbots.Domain;
using ChatbotDetailEntity = Nakama.ChatbotDetail.Domain.ChatbotDetail;

namespace Test.Modules.ChatbotDetail.Application;

[TestFixture]
public sealed class ChatbotDetailUpdatedOnChatbotTeamMemberRemovedShould : ChatbotDetailUnitTestCase
{
    private ChatbotDetailUpdatedOnChatbotTeamMemberRemoved _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotDetailUpdatedOnChatbotTeamMemberRemoved(this.DetailRepository.Object);
    }

    [Test]
    public async Task remove_the_member_row_at_the_index()
    {
        TeamMemberRow first = TeamMemberRowMother.Random();
        TeamMemberRow second = TeamMemberRowMother.Random();
        ChatbotDetailEntity detail = ChatbotDetailMother.With(team: [first, second]);
        ChatbotTeamMemberRemoved @event = ChatbotTeamMemberRemovedMother.With(ChatbotId.From(detail.Id), 1);
        this.ShouldFindChatbotDetail(detail);

        await this._handler.Handle(new DomainEventNotification<ChatbotTeamMemberRemoved>(@event), CancellationToken.None);

        detail.Team.Should().ContainSingle().Which.Should().BeSameAs(first);
        this.ShouldHaveUpdatedChatbotDetail(detail);
    }

    [Test]
    [TestCase(-1)]
    [TestCase(1)]
    public async Task leave_the_team_untouched_when_the_index_is_out_of_range(int index)
    {
        TeamMemberRow member = TeamMemberRowMother.Random();
        ChatbotDetailEntity detail = ChatbotDetailMother.With(team: [member]);
        ChatbotTeamMemberRemoved @event = ChatbotTeamMemberRemovedMother.With(ChatbotId.From(detail.Id), index);
        this.ShouldFindChatbotDetail(detail);

        await this._handler.Handle(new DomainEventNotification<ChatbotTeamMemberRemoved>(@event), CancellationToken.None);

        detail.Team.Should().ContainSingle().Which.Should().BeSameAs(member);
    }

    [Test]
    public async Task do_nothing_when_the_detail_does_not_exist()
    {
        ChatbotTeamMemberRemoved @event = ChatbotTeamMemberRemovedMother.Random();
        this.ShouldNotFindChatbotDetail();

        await this._handler.Handle(new DomainEventNotification<ChatbotTeamMemberRemoved>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyChatbotDetail();
    }
}
