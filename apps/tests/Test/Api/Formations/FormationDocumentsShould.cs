using System.Net;
using System.Net.Http.Json;
using System.Text;
using AwesomeAssertions;
using Nakama.FormationDetail.Application;
using Nakama.Formations.Domain;
using Test.Api.Support;
using Test.Modules.Shared;

namespace Test.Api.Formations;

[TestFixture]
public sealed class FormationDocumentsShould : ApiTestCase
{
    [Test]
    public async Task keep_the_uploaded_document_in_the_formation_and_let_the_administrator_download_it()
    {
        (string admin, Guid formation) = await this.CreateFormation();
        Guid document = Guid.NewGuid();
        byte[] content = Encoding.UTF8.GetBytes("Release notes");

        HttpResponseMessage upload = await this.Client.AddFormationDocument(formation, document, new AttachedFile("notes.txt", content), admin);

        upload.StatusCode.Should().Be(HttpStatusCode.Created);
        FormationDetailResponse? detail = await (await this.Client.GetFormation(formation, admin))
            .Content.ReadFromJsonAsync<FormationDetailResponse>();
        detail!.Docs.Should().ContainSingle().Which.Id.Should().Be(document);
        HttpResponseMessage download = await this.Client.DownloadFormationDocument(formation, document, admin);
        download.StatusCode.Should().Be(HttpStatusCode.OK);
        (await download.Content.ReadAsByteArrayAsync()).Should().Equal(content);
    }

    [Test]
    public async Task remove_the_document_from_the_formation()
    {
        (string admin, Guid formation) = await this.CreateFormation();
        Guid document = Guid.NewGuid();
        await this.Client.AddFormationDocument(formation, document, new AttachedFile("notes.txt", [1, 2, 3]), admin);

        HttpResponseMessage removal = await this.Client.RemoveFormationDocument(formation, document, admin);

        removal.StatusCode.Should().Be(HttpStatusCode.NoContent);
        FormationDetailResponse? detail = await (await this.Client.GetFormation(formation, admin))
            .Content.ReadFromJsonAsync<FormationDetailResponse>();
        detail!.Docs.Should().BeEmpty();
        (await this.Client.DownloadFormationDocument(formation, document, admin)).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task refuse_a_file_type_that_is_not_allowed()
    {
        (string admin, Guid formation) = await this.CreateFormation();

        HttpResponseMessage upload = await this.Client.AddFormationDocument(
            formation, Guid.NewGuid(), new AttachedFile("program.exe", [1, 2, 3]), admin);

        upload.IsSuccessStatusCode.Should().BeFalse();
        FormationDetailResponse? detail = await (await this.Client.GetFormation(formation, admin))
            .Content.ReadFromJsonAsync<FormationDetailResponse>();
        detail!.Docs.Should().BeEmpty();
    }

    [Test]
    public async Task answer_not_found_when_the_formation_does_not_exist()
    {
        string admin = await this.SignIn(isAdmin: true);

        HttpResponseMessage upload = await this.Client.AddFormationDocument(
            Guid.NewGuid(), Guid.NewGuid(), new AttachedFile("notes.txt", [1, 2, 3]), admin);

        upload.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task refuse_to_people_who_are_not_administrators_and_to_anonymous_callers()
    {
        (string admin, Guid formation) = await this.CreateFormation();
        Guid document = Guid.NewGuid();
        await this.Client.AddFormationDocument(formation, document, new AttachedFile("notes.txt", [1, 2, 3]), admin);
        string member = await this.SignIn(isAdmin: false);

        (await this.Client.AddFormationDocument(formation, Guid.NewGuid(), new AttachedFile("notes.txt", [1, 2, 3]), member))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await this.Client.RemoveFormationDocument(formation, document, member)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await this.Client.DownloadFormationDocument(formation, document, member)).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await this.Client.DownloadFormationDocument(formation, document, null)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
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
