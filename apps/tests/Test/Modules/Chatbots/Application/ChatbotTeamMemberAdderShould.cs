using AwesomeAssertions;
using Nakama.Chatbots.Application.TeamMemberAdder;
using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Test.Modules.Chatbots.Domain;

namespace Test.Modules.Chatbots.Application;

[TestFixture]
public sealed class ChatbotTeamMemberAdderShould : ChatbotsUnitTestCase
{
    private ChatbotTeamMemberAdder.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new ChatbotTeamMemberAdder.Handler(new(this.ChatbotsRepository.Object, this.EventBus.Object));
    }

    [Test]
    public async Task add_the_member_to_the_team_and_publish_team_member_added()
    {
        Chatbot chatbot = ChatbotMother.Random();
        ChatbotTeamMemberAdder.Command command = ChatbotTeamMemberAdderCommandMother.With(
            chatbot.Id.Value,
            role: TeamRole.Reader);
        TeamMember member = new(command.UserId, TeamRole.Reader);
        this.ShouldFindChatbot(chatbot);

        await this._handler.Handle(command, CancellationToken.None);

        chatbot.Team.Should().Contain(member);
        this.ShouldHaveSavedChatbot(chatbot);
        this.ShouldHavePublishedTeamMemberAdded(chatbot.Id, member);
    }

    [Test]
    public async Task add_the_member_when_the_team_already_has_other_users()
    {
        TeamMember owner = TeamMemberMother.With(role: TeamRole.Owner);
        TeamMember editor = TeamMemberMother.With(role: TeamRole.Editor);
        Chatbot chatbot = ChatbotMother.With(team: [owner, editor]);
        ChatbotTeamMemberAdder.Command command = ChatbotTeamMemberAdderCommandMother.With(chatbot.Id.Value);
        this.ShouldFindChatbot(chatbot);

        await this._handler.Handle(command, CancellationToken.None);

        chatbot.Team.Should().HaveCount(3);
        this.ShouldHaveSavedChatbot(chatbot);
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_user_is_already_in_the_team()
    {
        TeamMember existing = TeamMemberMother.With(role: TeamRole.Editor);
        Chatbot chatbot = ChatbotMother.With(team: [existing]);
        ChatbotTeamMemberAdder.Command command = ChatbotTeamMemberAdderCommandMother.With(
            chatbot.Id.Value,
            existing.UserId,
            TeamRole.Reader);
        this.ShouldFindChatbot(chatbot);

        Func<Task> act = async () => await this._handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ChatbotTeamMemberAlreadyExistsError>();
        chatbot.Team.Should().ContainSingle().Which.Should().Be(existing);
        this.ShouldNotHaveSavedAnyChatbot();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_chatbot_does_not_exist()
    {
        this.ShouldNotFindChatbot();

        Func<Task> act = async () =>
            await this._handler.Handle(ChatbotTeamMemberAdderCommandMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<ChatbotNotFoundError>();
        this.ShouldNotHaveSavedAnyChatbot();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_chatbot_is_archived()
    {
        Chatbot chatbot = ChatbotMother.Archived();
        this.ShouldFindChatbot(chatbot);

        Func<Task> act = async () =>
            await this._handler.Handle(ChatbotTeamMemberAdderCommandMother.With(chatbot.Id.Value), CancellationToken.None);

        await act.Should().ThrowAsync<ChatbotArchivedError>();
        this.ShouldNotHaveSavedAnyChatbot();
        this.ShouldNotHavePublishedAnything();
    }
}
