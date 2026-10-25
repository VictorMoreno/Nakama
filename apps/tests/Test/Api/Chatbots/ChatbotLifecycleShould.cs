using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Nakama.ChatbotDetail.Application;
using Nakama.ChatbotSummaries.Application;
using Nakama.Chatbots.Application.Creator;
using Nakama.Chatbots.Domain;
using Nakama.LlmProviders.Domain;
using Test.Api.Support;
using Test.Modules.Chatbots.Application;
using Test.Modules.Shared;

namespace Test.Api.Chatbots;

[TestFixture]
public sealed class ChatbotLifecycleShould : ApiTestCase
{
    [Test]
    public async Task create_a_chatbot_and_find_it_in_the_list_and_in_the_detail()
    {
        string token = await this.SignIn(isAdmin: true);
        ChatbotCreator.Command command = ChatbotCreatorCommandMother.Random();

        HttpResponseMessage created = await this.Client.CreateChatbot(command, token);

        created.StatusCode.Should().Be(HttpStatusCode.Created);
        List<ChatbotSummaryResponse>? chatbots = await (await this.Client.ListChatbots(token))
            .Content.ReadFromJsonAsync<List<ChatbotSummaryResponse>>();
        ChatbotSummaryResponse summary = chatbots.Should().ContainSingle(item => item.Id == command.Id).Which;
        summary.Label.Should().Be(command.Label);
        summary.Description.Should().Be(command.Description);
        summary.Status.Should().Be(((ChatbotStatus)command.Status).ToString());
        summary.DocsCount.Should().Be(0);
        summary.Archived.Should().BeFalse();
        ChatbotDetailResponse? detail = await (await this.Client.GetChatbot(command.Id, token))
            .Content.ReadFromJsonAsync<ChatbotDetailResponse>();
        detail.Should().NotBeNull();
        detail.Label.Should().Be(command.Label);
        detail.Description.Should().Be(command.Description);
        detail.Socratic.Should().Be(command.Socratic);
        detail.ExplicaMas.Should().Be(command.ExplicaMas);
        detail.Provider.Should().Be(((LlmProvider)command.Provider).ToString());
        detail.Model.Should().Be(command.Model);
        detail.Docs.Should().BeEmpty();
    }

    [Test]
    public async Task reject_a_second_chatbot_with_the_same_id_and_keep_the_first_one()
    {
        string token = await this.SignIn(isAdmin: true);
        ChatbotCreator.Command first = ChatbotCreatorCommandMother.Random();
        ChatbotCreator.Command second = ChatbotCreatorCommandMother.With(id: first.Id);
        await this.Client.CreateChatbot(first, token);

        HttpResponseMessage duplicated = await this.Client.CreateChatbot(second, token);

        duplicated.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await duplicated.Content.ReadFromJsonAsync<ProblemDetails>())!.Title.Should().Be("nakama.chatbots.already_exists");
        ChatbotDetailResponse? detail = await (await this.Client.GetChatbot(first.Id, token))
            .Content.ReadFromJsonAsync<ChatbotDetailResponse>();
        detail!.Label.Should().Be(first.Label);
    }

    [Test]
    public async Task archive_a_chatbot_and_see_it_archived_in_the_list_and_in_the_detail()
    {
        string token = await this.SignIn(isAdmin: true);
        ChatbotCreator.Command command = ChatbotCreatorCommandMother.Random();
        await this.Client.CreateChatbot(command, token);

        HttpResponseMessage archived = await this.Client.ArchiveChatbot(command.Id, token);

        archived.StatusCode.Should().Be(HttpStatusCode.NoContent);
        List<ChatbotSummaryResponse>? chatbots = await (await this.Client.ListChatbots(token))
            .Content.ReadFromJsonAsync<List<ChatbotSummaryResponse>>();
        chatbots.Should().ContainSingle(item => item.Id == command.Id).Which.Archived.Should().BeTrue();
        ChatbotDetailResponse? detail = await (await this.Client.GetChatbot(command.Id, token))
            .Content.ReadFromJsonAsync<ChatbotDetailResponse>();
        detail!.Archived.Should().BeTrue();
    }

    [Test]
    public async Task change_the_status_and_the_details_of_a_chatbot_and_see_them_in_the_list_and_in_the_detail()
    {
        string token = await this.SignIn(isAdmin: true);
        ChatbotCreator.Command command = ChatbotCreatorCommandMother.With(status: ChatbotStatus.Active);
        string label = WordMother.Random();
        string description = WordMother.Random();
        await this.Client.CreateChatbot(command, token);

        HttpResponseMessage updated = await this.Client.UpdateChatbotStatus(command.Id, (int)ChatbotStatus.Inactive, token);
        HttpResponseMessage detailsUpdated = await this.Client.UpdateChatbotDetails(command.Id, label, description, token);

        updated.StatusCode.Should().Be(HttpStatusCode.NoContent);
        detailsUpdated.StatusCode.Should().Be(HttpStatusCode.NoContent);
        List<ChatbotSummaryResponse>? chatbots = await (await this.Client.ListChatbots(token))
            .Content.ReadFromJsonAsync<List<ChatbotSummaryResponse>>();
        ChatbotSummaryResponse summary = chatbots.Should().ContainSingle(item => item.Id == command.Id).Which;
        summary.Status.Should().Be(nameof(ChatbotStatus.Inactive));
        summary.Label.Should().Be(label);
        summary.Description.Should().Be(description);
        ChatbotDetailResponse? detail = await (await this.Client.GetChatbot(command.Id, token))
            .Content.ReadFromJsonAsync<ChatbotDetailResponse>();
        detail!.Status.Should().Be(nameof(ChatbotStatus.Inactive));
        detail.Label.Should().Be(label);
        detail.Description.Should().Be(description);
    }

    [Test]
    public async Task answer_not_found_for_a_chatbot_that_does_not_exist()
    {
        string token = await this.SignIn(isAdmin: true);

        HttpResponseMessage response = await this.Client.GetChatbot(Guid.NewGuid(), token);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Title.Should().Be("nakama.chatbot_detail.not_found");
    }

    [Test]
    public async Task let_a_member_read_the_chatbots_but_not_create_them()
    {
        string memberToken = await this.SignIn(isAdmin: false);

        HttpResponseMessage list = await this.Client.ListChatbots(memberToken);
        HttpResponseMessage create = await this.Client.CreateChatbot(ChatbotCreatorCommandMother.Random(), memberToken);

        list.StatusCode.Should().Be(HttpStatusCode.OK);
        create.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task unarchive_an_archived_chatbot_and_see_it_active_again()
    {
        string token = await this.SignIn(isAdmin: true);
        ChatbotCreator.Command command = ChatbotCreatorCommandMother.Random();
        await this.Client.CreateChatbot(command, token);
        await this.Client.ArchiveChatbot(command.Id, token);

        HttpResponseMessage restored = await this.Client.UnarchiveChatbot(command.Id, token);

        restored.StatusCode.Should().Be(HttpStatusCode.NoContent);
        ChatbotDetailResponse? detail = await (await this.Client.GetChatbot(command.Id, token))
            .Content.ReadFromJsonAsync<ChatbotDetailResponse>();
        detail!.Archived.Should().BeFalse();
        List<ChatbotSummaryResponse>? chatbots = await (await this.Client.ListChatbots(token))
            .Content.ReadFromJsonAsync<List<ChatbotSummaryResponse>>();
        chatbots.Should().ContainSingle(item => item.Id == command.Id).Which.Archived.Should().BeFalse();
    }

    [Test]
    public async Task reject_changing_an_archived_chatbot_until_it_is_unarchived()
    {
        string token = await this.SignIn(isAdmin: true);
        ChatbotCreator.Command command = ChatbotCreatorCommandMother.Random();
        await this.Client.CreateChatbot(command, token);
        await this.Client.ArchiveChatbot(command.Id, token);

        HttpResponseMessage rejected = await this.Client.UpdateChatbotStatus(command.Id, (int)ChatbotStatus.Draft, token);

        rejected.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await rejected.Content.ReadFromJsonAsync<ProblemDetails>())!.Title.Should().Be("nakama.chatbots.archived");
        (await this.Client.UpdateChatbotDetails(command.Id, WordMother.Random(), WordMother.Random(), token))
            .StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        await this.Client.UnarchiveChatbot(command.Id, token);
        (await this.Client.UpdateChatbotStatus(command.Id, (int)ChatbotStatus.Draft, token)).StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task return_not_found_when_unarchiving_a_chatbot_that_does_not_exist()
    {
        string token = await this.SignIn(isAdmin: true);

        HttpResponseMessage response = await this.Client.UnarchiveChatbot(Guid.NewGuid(), token);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
