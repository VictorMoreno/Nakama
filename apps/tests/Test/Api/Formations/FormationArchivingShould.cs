using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Nakama.FormationDetail.Application;
using Nakama.Formations.Domain;
using Test.Api.Support;
using Test.Modules.Shared;

namespace Test.Api.Formations;

[TestFixture]
public sealed class FormationArchivingShould : ApiTestCase
{
    [Test]
    public async Task unarchive_an_archived_formation_and_edit_its_details_again()
    {
        (string admin, Guid formation) = await this.CreateFormation();
        string title = WordMother.Random();
        string audience = WordMother.Random();
        string description = WordMother.Random();
        await this.Client.ArchiveFormation(formation, admin);

        HttpResponseMessage restored = await this.Client.UnarchiveFormation(formation, admin);
        HttpResponseMessage detailsUpdated = await this.Client.UpdateFormationDetails(formation, title, audience, description, admin);

        restored.StatusCode.Should().Be(HttpStatusCode.NoContent);
        detailsUpdated.StatusCode.Should().Be(HttpStatusCode.NoContent);
        FormationDetailResponse? detail = await (await this.Client.GetFormation(formation, admin))
            .Content.ReadFromJsonAsync<FormationDetailResponse>();
        detail!.Archived.Should().BeFalse();
        detail.Title.Should().Be(title);
        detail.Audience.Should().Be(audience);
        detail.Description.Should().Be(description);
    }

    [Test]
    public async Task reject_uploading_a_document_and_editing_the_details_of_an_archived_formation()
    {
        (string admin, Guid formation) = await this.CreateFormation();
        await this.Client.ArchiveFormation(formation, admin);

        HttpResponseMessage rejected = await this.Client.AddFormationDocument(
            formation, Guid.NewGuid(), new AttachedFile("notes.txt", [1, 2, 3]), admin);
        HttpResponseMessage detailsRejected = await this.Client.UpdateFormationDetails(
            formation, WordMother.Random(), WordMother.Random(), WordMother.Random(), admin);

        rejected.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await rejected.Content.ReadFromJsonAsync<ProblemDetails>())!.Title.Should().Be("nakama.formations.archived");
        detailsRejected.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    public async Task return_not_found_when_unarchiving_a_formation_that_does_not_exist()
    {
        string admin = await this.SignIn(isAdmin: true);

        HttpResponseMessage response = await this.Client.UnarchiveFormation(Guid.NewGuid(), admin);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<(string Admin, Guid Formation)> CreateFormation()
    {
        string admin = await this.SignIn(isAdmin: true);
        Guid formation = Guid.NewGuid();
        (await this.Client.CreateFormation(formation, WordMother.Random(), WordMother.Random(), WordMother.Random(), FormationStatus.Active, admin))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        return (admin, formation);
    }
}
