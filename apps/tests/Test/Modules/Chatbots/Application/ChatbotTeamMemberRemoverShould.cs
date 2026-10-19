using AwesomeAssertions;
using Nakama.Chatbots.Application.TeamMemberRemover;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Nakama.Chatbots.Domain.Events;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.Chatbots.Application;

[TestFixture]
public sealed class ChatbotTeamMemberRemoverShould : ChatbotsUnitTestCase
{
    private ChatbotTeamMemberRemover.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotTeamMemberRemover.Handler(new(this.ChatbotsRepository.Object, this.EventBus.Object));
    }

    [Test]
    public async Task remove_the_member_at_the_index_and_publish_team_member_removed()
    {
        TeamMember owner = TeamMemberMother.With(role: TeamRole.Owner);
        TeamMember editor = TeamMemberMother.With(role: TeamRole.Editor);
        Chatbot chatbot = ChatbotMother.With(team: [owner, editor]);
        this.ShouldFindChatbot(chatbot);

        await this._handler.Handle(ChatbotTeamMemberRemoverCommandMother.With(chatbot.Id.Value, 1), CancellationToken.None);

        chatbot.Team.Should().ContainSingle().Which.Should().Be(owner);
        this.ShouldHaveSavedChatbot(chatbot);
        this.ShouldHavePublished<ChatbotTeamMemberRemoved>(
            @event => @event.Id == chatbot.Id && @event.Index == 1 && @event.UserId == editor.UserId);
    }

    [Test]
    public async Task fail_without_removing_anything_when_the_member_is_the_owner()
    {
        TeamMember owner = TeamMemberMother.With(role: TeamRole.Owner);
        TeamMember editor = TeamMemberMother.With(role: TeamRole.Editor);
        Chatbot chatbot = ChatbotMother.With(team: [owner, editor]);
        this.ShouldFindChatbot(chatbot);

        Func<Task> act = async () => await this._handler.Handle(
            ChatbotTeamMemberRemoverCommandMother.With(chatbot.Id.Value, 0),
            CancellationToken.None);

        await act.Should().ThrowAsync<ChatbotOwnerCannotBeRemovedError>();
        chatbot.Team.Should().Equal(owner, editor);
        this.ShouldNotHaveSavedAnyChatbot();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    [TestCase(-1)]
    [TestCase(1)]
    public async Task fail_without_saving_anything_when_the_index_is_out_of_range(int index)
    {
        TeamMember owner = TeamMemberMother.With(role: TeamRole.Owner);
        Chatbot chatbot = ChatbotMother.With(team: [owner]);
        this.ShouldFindChatbot(chatbot);

        Func<Task> act = async () => await this._handler.Handle(
            ChatbotTeamMemberRemoverCommandMother.With(chatbot.Id.Value, index),
            CancellationToken.None);

        await act.Should().ThrowAsync<ChatbotTeamMemberNotFoundError>();
        chatbot.Team.Should().Equal(owner);
        this.ShouldNotHaveSavedAnyChatbot();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_chatbot_does_not_exist()
    {
        this.ShouldNotFindChatbot();

        Func<Task> act = async () =>
            await this._handler.Handle(ChatbotTeamMemberRemoverCommandMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<ChatbotNotFoundError>();
        this.ShouldNotHaveSavedAnyChatbot();
        this.ShouldNotHavePublishedAnything();
    }
}
