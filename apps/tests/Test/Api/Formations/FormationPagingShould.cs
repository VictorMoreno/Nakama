using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Nakama.Formations.Domain;
using Nakama.FormationsList.Application;
using Nakama.Shared.Application;
using Test.Api.Support;

namespace Test.Api.Formations;

[TestFixture]
public sealed class FormationPagingShould : ApiTestCase
{
    [Test]
    public async Task return_the_formations_page_by_page_ordered_by_title_with_the_total()
    {
        string token = await this.SignIn(isAdmin: true);
        await this.CreateFormation(token, "Echo");
        await this.CreateFormation(token, "Alpha");
        await this.CreateFormation(token, "Charlie");
        await this.CreateFormation(token, "Bravo");

        PagedResponse<FormationSummaryResponse> first = await this.ReadPage(token, page: 1, pageSize: 3);
        PagedResponse<FormationSummaryResponse> second = await this.ReadPage(token, page: 2, pageSize: 3);

        first.Items.Select(item => item.Title).Should().Equal("Alpha", "Bravo", "Charlie");
        first.TotalCount.Should().Be(4);
        first.TotalPages.Should().Be(2);
        second.Items.Select(item => item.Title).Should().Equal("Echo");
        second.Page.Should().Be(2);
    }

    [Test]
    public async Task filter_by_search_text_in_title_audience_or_description_ignoring_case()
    {
        string token = await this.SignIn(isAdmin: true);
        await this.CreateFormation(token, "Seguridad vial", audience: "Conductores");
        await this.CreateFormation(token, "Onboarding", audience: "Equipo de SEGURIDAD");
        await this.CreateFormation(token, "Ofimática", description: "Curso de Seguridad digital");
        await this.CreateFormation(token, "Liderazgo");

        PagedResponse<FormationSummaryResponse> response = await this.ReadPage(token, search: "seguridad");

        response.Items.Select(item => item.Title).Should().Equal("Ofimática", "Onboarding", "Seguridad vial");
        response.TotalCount.Should().Be(3);
    }

    [Test]
    public async Task filter_by_status()
    {
        string token = await this.SignIn(isAdmin: true);
        await this.CreateFormation(token, "Activa", status: FormationStatus.Active);
        await this.CreateFormation(token, "Borrador", status: FormationStatus.Draft);

        PagedResponse<FormationSummaryResponse> response = await this.ReadPage(token, status: "Draft");

        response.Items.Select(item => item.Title).Should().Equal("Borrador");
    }

    [Test]
    public async Task leave_archived_formations_out_of_the_page_and_the_total()
    {
        string token = await this.SignIn(isAdmin: true);
        Guid archived = await this.CreateFormation(token, "Archivada");
        await this.CreateFormation(token, "Vigente");
        await this.Client.ArchiveFormation(archived, token);

        PagedResponse<FormationSummaryResponse> response = await this.ReadPage(token);

        response.Items.Select(item => item.Title).Should().Equal("Vigente");
        response.TotalCount.Should().Be(1);
    }

    [Test]
    public async Task show_a_regular_user_an_empty_page_when_they_are_not_assigned_to_any_formation()
    {
        string adminToken = await this.SignIn(isAdmin: true);
        await this.CreateFormation(adminToken, "Alpha");
        string userToken = await this.SignIn(isAdmin: false);

        PagedResponse<FormationSummaryResponse> response = await this.ReadPage(userToken);

        response.Items.Should().BeEmpty();
        response.TotalCount.Should().Be(0);
    }

    [Test]
    public async Task reject_a_page_size_above_the_maximum()
    {
        string token = await this.SignIn(isAdmin: true);

        HttpResponseMessage response = await this.Client.ListFormationsPage(token, pageSize: 1000);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Title.Should().Be("nakama.pagination.invalid_page_size");
    }

    private async Task<Guid> CreateFormation(
        string token,
        string title,
        string audience = "Todos",
        string description = "x",
        FormationStatus status = FormationStatus.Active)
    {
        Guid id = Guid.NewGuid();
        HttpResponseMessage created = await this.Client.CreateFormation(id, title, audience, description, status, token);
        created.StatusCode.Should().Be(HttpStatusCode.Created);

        return id;
    }

    private async Task<PagedResponse<FormationSummaryResponse>> ReadPage(
        string token,
        int? page = null,
        int? pageSize = null,
        string? search = null,
        string? status = null)
    {
        HttpResponseMessage response = await this.Client.ListFormationsPage(token, page, pageSize, search, status);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return (await response.Content.ReadFromJsonAsync<PagedResponse<FormationSummaryResponse>>())!;
    }
}
