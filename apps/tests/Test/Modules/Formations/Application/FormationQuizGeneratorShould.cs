using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Nakama.FormationDetail.Application;
using Nakama.Formations.Application;
using Nakama.Formations.Application.QuizGenerator;
using Nakama.Formations.Domain.Errors;
using Nakama.LlmProviders.Domain;
using Nakama.Shared.Domain;
using Test.Api.Support;
using Test.Modules.Chatbots;
using Test.Modules.FormationDetail.Application;
using Test.Modules.Shared;

namespace Test.Modules.Formations.Application;

[TestFixture]
public sealed class FormationQuizGeneratorShould : FormationsUnitTestCase
{
    private const string ValidAnswer =
        "[{\"question\":\"  What is a cat?  \",\"options\":[\"A pet\",\"A rock\",\"A car\",\"A star\"],\"correctIndex\":0}," +
        "{\"question\":\"What is a dog?\",\"options\":[\"A pet\",\"A rock\",\"A car\",\"A star\"],\"correctIndex\":0}]";

    private FakeLlmClient _llm = null!;

    private FormationQuizGenerator.Handler _handler = null!;

    private Guid _formationId;

    private FormationDocumentResponse _first = null!;

    private FormationDocumentResponse _second = null!;

    [SetUp]
    public void SetUp()
    {
        this._llm = new FakeLlmClient();
        ServiceProvider services = new ServiceCollection()
            .AddKeyedSingleton<ILlmClient>(LlmProvider.Anthropic.ToString(), this._llm)
            .BuildServiceProvider();
        this._handler = new FormationQuizGenerator.Handler(new(this.QueryBus.Object, services));
        this._formationId = Guid.NewGuid();
        this._first = FormationDocumentResponseMother.With(fileName: "first.pdf");
        this._second = FormationDocumentResponseMother.With(fileName: "second.md");
        this.ShouldFindFormationDocuments(this._first, this._second);
        this.ShouldFindDocumentFile(this._first, "application/pdf");
        this.ShouldFindDocumentFile(this._second, "text/markdown");
        this._llm.Answer(ValidAnswer);
    }

    [Test]
    public async Task return_the_questions_the_model_wrote_without_surrounding_spaces()
    {
        GeneratedQuizResponse response = await this._handler.Handle(this.QueryFor(2), CancellationToken.None);

        response.Questions.Should().HaveCount(2);
        response.Questions[0].Question.Should().Be("What is a cat?");
        response.Questions[0].Options.Should().Equal("A pet", "A rock", "A car", "A star");
        response.Questions[0].CorrectIndex.Should().Be(0);
    }

    [Test]
    public async Task accept_the_answer_when_the_model_wraps_it_in_a_code_fence()
    {
        this._llm.Answer("```json\n" + ValidAnswer + "\n```");

        GeneratedQuizResponse response = await this._handler.Handle(this.QueryFor(2), CancellationToken.None);

        response.Questions.Should().HaveCount(2);
    }

    [Test]
    public async Task ask_the_model_chosen_with_only_the_selected_documents_and_the_topic()
    {
        await this._handler.Handle(this.QueryFor(2, documents: [this._second.Id], model: "the-model", topic: "Deployments"), CancellationToken.None);

        LlmRequest request = this._llm.Requests.Should().ContainSingle().Subject;
        request.Model.Should().Be("the-model");
        request.SystemPrompt.Should().Contain("exactamente 2 preguntas").And.Contain("Deployments");
        request.Documents.Should().ContainSingle().Which.Name.Should().Be("second.md");
    }

    [Test]
    public async Task fail_when_the_model_does_not_answer_json()
    {
        this._llm.Answer("Sure, here you have some questions");

        Func<Task> act = async () => await this._handler.Handle(this.QueryFor(2), CancellationToken.None);

        await act.Should().ThrowAsync<FormationQuizGenerationFailedError>();
    }

    [Test]
    public async Task fail_when_the_model_returns_another_number_of_questions()
    {
        Func<Task> act = async () => await this._handler.Handle(this.QueryFor(3), CancellationToken.None);

        await act.Should().ThrowAsync<FormationQuizGenerationFailedError>();
    }

    [TestCase("[{\"question\":\"Q\",\"options\":[\"A\",\"B\"],\"correctIndex\":2}]")]
    [TestCase("[{\"question\":\"Q\",\"options\":[\"A\",\"B\"],\"correctIndex\":-1}]")]
    [TestCase("[{\"question\":\"Q\",\"options\":[\"A\"],\"correctIndex\":0}]")]
    [TestCase("[{\"question\":\"Q\",\"options\":[\"A\",\" \"],\"correctIndex\":0}]")]
    [TestCase("[{\"question\":\" \",\"options\":[\"A\",\"B\"],\"correctIndex\":0}]")]
    [TestCase("[{\"options\":[\"A\",\"B\"],\"correctIndex\":0}]")]
    public async Task fail_when_a_question_is_not_valid(string answer)
    {
        this._llm.Answer(answer);

        Func<Task> act = async () => await this._handler.Handle(this.QueryFor(1), CancellationToken.None);

        await act.Should().ThrowAsync<FormationQuizGenerationFailedError>();
    }

    [TestCase(0)]
    [TestCase(5)]
    public async Task fail_without_asking_the_model_when_the_number_of_questions_is_out_of_range(int count)
    {
        Func<Task> act = async () => await this._handler.Handle(this.QueryFor(count), CancellationToken.None);

        await act.Should().ThrowAsync<FormationInvalidQuizQuestionCountError>();
        this._llm.Requests.Should().BeEmpty();
    }

    [TestCase("")]
    [TestCase("   ")]
    public async Task fail_without_asking_the_model_when_there_is_no_model(string model)
    {
        Func<Task> act = async () => await this._handler.Handle(this.QueryFor(2, model: model), CancellationToken.None);

        await act.Should().ThrowAsync<FormationQuizModelRequiredError>();
        this._llm.Requests.Should().BeEmpty();
    }

    [Test]
    public async Task fail_when_the_provider_has_no_connected_client()
    {
        Func<Task> act = async () => await this._handler.Handle(this.QueryFor(2, provider: LlmProvider.Local), CancellationToken.None);

        await act.Should().ThrowAsync<FormationQuizProviderNotConnectedError>();
        this._llm.Requests.Should().BeEmpty();
    }

    [Test]
    public async Task fail_when_the_formation_has_no_documents()
    {
        this.ShouldFindFormationDocuments();

        Func<Task> act = async () => await this._handler.Handle(this.QueryFor(2), CancellationToken.None);

        await act.Should().ThrowAsync<FormationQuizWithoutDocumentsError>();
        this._llm.Requests.Should().BeEmpty();
    }

    [Test]
    public async Task fail_when_no_document_is_selected()
    {
        Func<Task> act = async () => await this._handler.Handle(this.QueryFor(2, documents: []), CancellationToken.None);

        await act.Should().ThrowAsync<FormationQuizNoDocumentsSelectedError>();
        this._llm.Requests.Should().BeEmpty();
    }

    [Test]
    public async Task fail_when_a_selected_document_is_not_from_the_formation()
    {
        Func<Task> act = async () =>
            await this._handler.Handle(this.QueryFor(2, documents: [this._first.Id, Guid.NewGuid()]), CancellationToken.None);

        await act.Should().ThrowAsync<FormationQuizDocumentNotInFormationError>();
        this._llm.Requests.Should().BeEmpty();
    }

    private FormationQuizGenerator.Query QueryFor(
        int count,
        IReadOnlyList<Guid>? documents = null,
        string? model = null,
        string? topic = null,
        LlmProvider provider = LlmProvider.Anthropic) =>
        new(
            this._formationId,
            documents ?? [this._first.Id, this._second.Id],
            (int)provider,
            model ?? WordMother.Random(),
            count,
            topic ?? WordMother.Random());

    private void ShouldFindFormationDocuments(params FormationDocumentResponse[] documents) =>
        this.ShouldFindFormationDetail(FormationDetailResponseMother.With(id: this._formationId, docs: documents.ToList()));
}
