using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Nakama.ChatbotSummaries.Application;
using Nakama.Chatbots.Application.Creator;
using Nakama.Chatbots.Domain;
using Nakama.Shared.Application;
using Test.Api.Support;
using Test.Modules.Chatbots.Application;

namespace Test.Api.Chatbots;

[TestFixture]
public sealed class ChatbotPagingShould : ApiTestCase
{
    [Test]
    public async Task return_the_chatbots_page_by_page_ordered_by_label_with_the_total()
    {
        string token = await this.SignIn(isAdmin: true);
        await this.CreateChatbots(token, "Echo", "Alpha", "Delta", "Bravo", "Charlie");

        PagedResponse<ChatbotSummaryResponse> first = await this.ReadPage(token, page: 1, pageSize: 2);
        PagedResponse<ChatbotSummaryResponse> last = await this.ReadPage(token, page: 3, pageSize: 2);
        PagedResponse<ChatbotSummaryResponse> beyond = await this.ReadPage(token, page: 4, pageSize: 2);

        first.Items.Select(item => item.Label).Should().Equal("Alpha", "Bravo");
        first.TotalCount.Should().Be(5);
        first.TotalPages.Should().Be(3);
        first.Page.Should().Be(1);
        first.PageSize.Should().Be(2);
        last.Items.Select(item => item.Label).Should().Equal("Echo");
        beyond.Items.Should().BeEmpty();
        beyond.TotalCount.Should().Be(5);
    }

    [Test]
    public async Task use_the_configured_default_page_size_when_none_is_requested()
    {
        string token = await this.SignIn(isAdmin: true);
        await this.CreateChatbots(token, "Alpha");

        PagedResponse<ChatbotSummaryResponse> response = await this.ReadPage(token);

        response.Page.Should().Be(1);
        response.PageSize.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task filter_by_search_text_in_label_or_description_ignoring_case()
    {
        string token = await this.SignIn(isAdmin: true);
        await this.CreateChatbot(token, "Soporte técnico", "Incidencias");
        await this.CreateChatbot(token, "RRHH", "Política de VACACIONES");
        await this.CreateChatbot(token, "Finanzas", "Facturas");

        PagedResponse<ChatbotSummaryResponse> byLabel = await this.ReadPage(token, search: "SOPORTE");
        PagedResponse<ChatbotSummaryResponse> byDescription = await this.ReadPage(token, search: "vacaciones");

        byLabel.Items.Select(item => item.Label).Should().Equal("Soporte técnico");
        byLabel.TotalCount.Should().Be(1);
        byDescription.Items.Select(item => item.Label).Should().Equal("RRHH");
    }

    [Test]
    public async Task filter_by_status()
    {
        string token = await this.SignIn(isAdmin: true);
        await this.CreateChatbot(token, "Activo", "x", ChatbotStatus.Active);
        await this.CreateChatbot(token, "Borrador", "x", ChatbotStatus.Draft);

        PagedResponse<ChatbotSummaryResponse> response = await this.ReadPage(token, status: "Draft");

        response.Items.Select(item => item.Label).Should().Equal("Borrador");
        response.TotalCount.Should().Be(1);
    }

    [Test]
    public async Task leave_archived_chatbots_out_of_the_page_and_the_total()
    {
        string token = await this.SignIn(isAdmin: true);
        ChatbotCreator.Command archived = await this.CreateChatbot(token, "Archivado", "x");
        await this.CreateChatbot(token, "Vigente", "x");
        await this.Client.ArchiveChatbot(archived.Id, token);

        PagedResponse<ChatbotSummaryResponse> response = await this.ReadPage(token);

        response.Items.Select(item => item.Label).Should().Equal("Vigente");
        response.TotalCount.Should().Be(1);
    }

    [Test]
    public async Task let_an_administrator_list_only_the_archived_chatbots()
    {
        string token = await this.SignIn(isAdmin: true);
        ChatbotCreator.Command archived = await this.CreateChatbot(token, "Archivado", "x");
        await this.CreateChatbot(token, "Vigente", "x");
        await this.Client.ArchiveChatbot(archived.Id, token);

        PagedResponse<ChatbotSummaryResponse> response = await this.ReadPage(token, archived: true);

        response.Items.Select(item => item.Label).Should().Equal("Archivado");
        response.TotalCount.Should().Be(1);
    }

    [Test]
    public async Task ignore_the_archived_filter_for_a_regular_user()
    {
        string adminToken = await this.SignIn(isAdmin: true);
        SignedInUser member = await this.SignInAs(isAdmin: false);
        ChatbotCreator.Command archived = await this.CreateChatbot(adminToken, "Archivado", "x");
        await this.Client.AddChatbotTeamMember(archived.Id, member.Id, TeamRole.Reader, adminToken);
        await this.Client.ArchiveChatbot(archived.Id, adminToken);

        PagedResponse<ChatbotSummaryResponse> response = await this.ReadPage(member.Token, archived: true);

        response.Items.Should().BeEmpty();
    }

    [Test]
    public async Task show_a_regular_user_only_the_chatbots_they_belong_to()
    {
        string adminToken = await this.SignIn(isAdmin: true);
        SignedInUser member = await this.SignInAs(isAdmin: false);
        ChatbotCreator.Command mine = await this.CreateChatbot(adminToken, "Mío", "x");
        await this.CreateChatbot(adminToken, "Ajeno", "x");
        await this.Client.AddChatbotTeamMember(mine.Id, member.Id, TeamRole.Reader, adminToken);

        PagedResponse<ChatbotSummaryResponse> response = await this.ReadPage(member.Token);

        response.Items.Select(item => item.Label).Should().Equal("Mío");
        response.TotalCount.Should().Be(1);
    }

    [TestCase(0, null)]
    [TestCase(-1, null)]
    public async Task reject_an_invalid_page_number(int page, int? pageSize)
    {
        string token = await this.SignIn(isAdmin: true);

        HttpResponseMessage response = await this.Client.ListChatbotsPage(token, page: page, pageSize: pageSize);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Title.Should().Be("nakama.pagination.invalid_page");
    }

    [TestCase(0)]
    [TestCase(1000)]
    public async Task reject_a_page_size_outside_the_allowed_range(int pageSize)
    {
        string token = await this.SignIn(isAdmin: true);

        HttpResponseMessage response = await this.Client.ListChatbotsPage(token, pageSize: pageSize);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Title.Should().Be("nakama.pagination.invalid_page_size");
    }

    [Test]
    public async Task require_authentication()
    {
        HttpResponseMessage response = await this.Client.GetAsync("/api/chatbots/paged");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<ChatbotCreator.Command> CreateChatbot(
        string token,
        string label,
        string description,
        ChatbotStatus status = ChatbotStatus.Active)
    {
        ChatbotCreator.Command command = ChatbotCreatorCommandMother.With(label: label, description: description, status: status);
        HttpResponseMessage created = await this.Client.CreateChatbot(command, token);
        created.StatusCode.Should().Be(HttpStatusCode.Created);

        return command;
    }

    private async Task CreateChatbots(string token, params string[] labels)
    {
        foreach (string label in labels)
        {
            await this.CreateChatbot(token, label, "x");
        }
    }

    private async Task<PagedResponse<ChatbotSummaryResponse>> ReadPage(
        string token,
        int? page = null,
        int? pageSize = null,
        string? search = null,
        string? status = null,
        bool? archived = null)
    {
        HttpResponseMessage response = await this.Client.ListChatbotsPage(token, page, pageSize, search, status, archived);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return (await response.Content.ReadFromJsonAsync<PagedResponse<ChatbotSummaryResponse>>())!;
    }
}
