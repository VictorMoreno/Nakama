using System.Net;
using System.Net.Http.Json;
using System.Text;
using AwesomeAssertions;
using Nakama.Formations.Application;
using Nakama.Formations.Domain;
using Nakama.LlmProviders.Domain;
using Test.Api.Support;
using Test.Modules.Shared;

namespace Test.Api.Formations;

[TestFixture]
public sealed class FormationQuizGenerationShould : ApiTestCase
{
    private const string Answer =
        "[{\"question\":\"What is a cat?\",\"options\":[\"A pet\",\"A rock\",\"A car\",\"A star\"],\"correctIndex\":0}," +
        "{\"question\":\"What is a dog?\",\"options\":[\"A pet\",\"A rock\",\"A car\",\"A star\"],\"correctIndex\":0}]";

    [Test]
    public async Task generate_a_draft_from_the_selected_documents_of_the_formation_with_the_chosen_model()
    {
        (string admin, Guid formation, Guid document) = await this.CreateFormationWithDocument();
        this.Llm.Answer(Answer);

        HttpResponseMessage response = await this.Client.GenerateQuizQuestions(
            formation, [document], (int)LlmProvider.Anthropic, "the-model", 2, "Pets", admin);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        GeneratedQuizResponse? quiz = await response.Content.ReadFromJsonAsync<GeneratedQuizResponse>();
        quiz!.Questions.Select(question => question.Question).Should().Equal("What is a cat?", "What is a dog?");
        this.Llm.Requests.Should().ContainSingle().Which.Model.Should().Be("the-model");
        this.Llm.Requests[0].Documents.Should().ContainSingle();
    }

    [Test]
    public async Task answer_unprocessable_when_the_model_does_not_return_valid_questions()
    {
        (string admin, Guid formation, Guid document) = await this.CreateFormationWithDocument();
        this.Llm.Answer("not json");

        HttpResponseMessage response = await this.Client.GenerateQuizQuestions(
            formation, [document], (int)LlmProvider.Anthropic, "the-model", 2, "Pets", admin);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }

    [Test]
    public async Task answer_unprocessable_when_the_formation_has_no_documents()
    {
        string admin = await this.SignIn(isAdmin: true);
        Guid formation = await this.CreateFormation(admin);

        HttpResponseMessage response = await this.Client.GenerateQuizQuestions(
            formation, [Guid.NewGuid()], (int)LlmProvider.Anthropic, "the-model", 2, "Pets", admin);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        this.Llm.Requests.Should().BeEmpty();
    }

    [Test]
    public async Task answer_unprocessable_when_more_questions_than_the_maximum_are_requested()
    {
        (string admin, Guid formation, Guid document) = await this.CreateFormationWithDocument();

        HttpResponseMessage response = await this.Client.GenerateQuizQuestions(
            formation, [document], (int)LlmProvider.Anthropic, "the-model", 5, "Pets", admin);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        this.Llm.Requests.Should().BeEmpty();
    }

    [Test]
    public async Task answer_not_found_when_the_formation_does_not_exist()
    {
        string admin = await this.SignIn(isAdmin: true);

        HttpResponseMessage response = await this.Client.GenerateQuizQuestions(
            Guid.NewGuid(), [Guid.NewGuid()], (int)LlmProvider.Anthropic, "the-model", 2, "Pets", admin);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task refuse_to_people_who_are_not_administrators_and_to_anonymous_callers()
    {
        string member = await this.SignIn(isAdmin: false);

        (await this.Client.GenerateQuizQuestions(Guid.NewGuid(), [Guid.NewGuid()], 0, "the-model", 2, "Pets", member))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await this.Client.SendJson(HttpMethod.Post, $"/api/formations/{Guid.NewGuid()}/quiz-questions", new { }, string.Empty))
            .StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<Guid> CreateFormation(string admin)
    {
        Guid formation = Guid.NewGuid();
        (await this.Client.CreateFormation(formation, WordMother.Random(), WordMother.Random(), WordMother.Random(), FormationStatus.Active, admin))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        return formation;
    }

    private async Task<(string Admin, Guid Formation, Guid Document)> CreateFormationWithDocument()
    {
        string admin = await this.SignIn(isAdmin: true);
        Guid formation = await this.CreateFormation(admin);
        Guid document = Guid.NewGuid();
        (await this.Client.AddFormationDocument(formation, document, new AttachedFile("pets.txt", Encoding.UTF8.GetBytes(WordMother.Random())), admin))
            .StatusCode.Should().Be(HttpStatusCode.Created);

        return (admin, formation, document);
    }
}
