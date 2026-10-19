using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Nakama.ChatbotSummaries.Application;
using Nakama.Chatbots.Application.Creator;
using Nakama.Chatbots.Domain;
using Test.Api.Support;
using Test.Modules.Chatbots.Application;

namespace Test.Api.Chatbots;

[TestFixture]
public sealed class ChatbotAccessShould : ApiTestCase
{
    [Test]
    public async Task let_a_member_see_only_the_chatbots_whose_team_they_belong_to()
    {
        string adminToken = await this.SignIn(isAdmin: true);
        SignedInUser member = await this.SignInAs(isAdmin: false);
        ChatbotCreator.Command mine = ChatbotCreatorCommandMother.Random();
        ChatbotCreator.Command notMine = ChatbotCreatorCommandMother.Random();
        await this.Client.CreateChatbot(mine, adminToken);
        await this.Client.CreateChatbot(notMine, adminToken);

        HttpResponseMessage added = await this.Client.AddChatbotTeamMember(mine.Id, member.Id, TeamRole.Editor, adminToken);

        added.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await this.ListedIds(member.Token)).Should().Equal(mine.Id);
    }

    [Test]
    public async Task let_an_administrator_see_every_chatbot_whatever_its_team()
    {
        string adminToken = await this.SignIn(isAdmin: true);
        ChatbotCreator.Command first = ChatbotCreatorCommandMother.Random();
        ChatbotCreator.Command second = ChatbotCreatorCommandMother.Random();
        await this.Client.CreateChatbot(first, adminToken);
        await this.Client.CreateChatbot(second, adminToken);

        List<Guid> listed = await this.ListedIds(adminToken);

        listed.Should().BeEquivalentTo([first.Id, second.Id]);
    }

    [Test]
    public async Task show_no_chatbots_to_a_member_that_belongs_to_no_team()
    {
        string adminToken = await this.SignIn(isAdmin: true);
        string memberToken = await this.SignIn(isAdmin: false);
        await this.Client.CreateChatbot(ChatbotCreatorCommandMother.Random(), adminToken);

        (await this.ListedIds(memberToken)).Should().BeEmpty();
    }

    [Test]
    public async Task stop_showing_a_chatbot_to_a_member_removed_from_its_team()
    {
        string adminToken = await this.SignIn(isAdmin: true);
        SignedInUser member = await this.SignInAs(isAdmin: false);
        ChatbotCreator.Command command = ChatbotCreatorCommandMother.Random();
        await this.Client.CreateChatbot(command, adminToken);
        await this.Client.AddChatbotTeamMember(command.Id, member.Id, TeamRole.Reader, adminToken);

        HttpResponseMessage removed = await this.Client.RemoveChatbotTeamMember(command.Id, 0, adminToken);

        removed.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await this.ListedIds(member.Token)).Should().BeEmpty();
    }

    [Test]
    public async Task keep_showing_the_chatbot_to_the_members_that_remain_when_another_one_is_removed()
    {
        string adminToken = await this.SignIn(isAdmin: true);
        SignedInUser staying = await this.SignInAs(isAdmin: false);
        SignedInUser leaving = await this.SignInAs(isAdmin: false);
        ChatbotCreator.Command command = ChatbotCreatorCommandMother.Random();
        await this.Client.CreateChatbot(command, adminToken);
        await this.Client.AddChatbotTeamMember(command.Id, staying.Id, TeamRole.Editor, adminToken);
        await this.Client.AddChatbotTeamMember(command.Id, leaving.Id, TeamRole.Reader, adminToken);

        HttpResponseMessage removed = await this.Client.RemoveChatbotTeamMember(command.Id, 1, adminToken);

        removed.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await this.ListedIds(leaving.Token)).Should().BeEmpty();
        (await this.ListedIds(staying.Token)).Should().Equal(command.Id);
    }

    private async Task<List<Guid>> ListedIds(string token)
    {
        List<ChatbotSummaryResponse>? chatbots = await (await this.Client.ListChatbots(token))
            .Content.ReadFromJsonAsync<List<ChatbotSummaryResponse>>();

        return chatbots!.Select(item => item.Id).ToList();
    }
}
