using AwesomeAssertions;
using Nakama.Formations.Application.ModuleAdder;
using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Nakama.Formations.Domain.Events;
using Test.Modules.Formations.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Formations.Application;

[TestFixture]
public sealed class FormationModuleAdderShould : FormationsUnitTestCase
{
    private FormationModuleAdder.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationModuleAdder.Handler(new(this.FormationsRepository.Object, this.EventBus.Object));
    }

    [Test]
    public async Task add_the_module_and_publish_formation_module_added()
    {
        Formation formation = FormationMother.Random();
        FormationModuleAdder.Command command = FormationModuleAdderCommandMother.With(
            formation.Id.Value,
            type: ModuleType.Video,
            gated: true,
            aiDraft: true,
            resourceUrl: WordMother.Random(),
            durationMin: 12,
            passingScore: null);
        this.ShouldFindFormation(formation);

        await this._handler.Handle(command, CancellationToken.None);

        formation.Modules.Should().ContainSingle().Which.Should().Be(new FormationModule(
            ModuleId.From(command.ModuleId),
            command.Title,
            ModuleType.Video,
            command.Summary,
            true,
            true,
            command.ResourceUrl,
            12,
            null,
            null));
        this.ShouldHaveSavedFormation(formation);
        this.ShouldHavePublished<FormationModuleAdded>(
            @event => @event.Id == formation.Id && @event.Module.Id.Value == command.ModuleId);
    }

    [Test]
    public async Task map_the_quiz_questions_of_the_command_into_the_module()
    {
        string question = WordMother.Random();
        string firstOption = WordMother.Random();
        string secondOption = WordMother.Random();
        Formation formation = FormationMother.Random();
        FormationModuleAdder.Command command = FormationModuleAdderCommandMother.With(
            formation.Id.Value,
            type: ModuleType.Exam,
            quiz: [new FormationModuleAdder.QuizQuestionInput(question, [firstOption, secondOption], 1)],
            passingScore: 80);
        this.ShouldFindFormation(formation);

        await this._handler.Handle(command, CancellationToken.None);

        formation.Modules.Should().ContainSingle().Which.Quiz.Should().BeEquivalentTo(
            new[] { new QuizQuestion(question, [firstOption, secondOption], 1) });
        formation.Modules.Should().ContainSingle().Which.PassingScore.Should().Be(80);
    }

    [Test]
    public async Task add_the_module_when_the_formation_already_has_other_modules()
    {
        FormationModule existing = FormationModuleMother.Random();
        Formation formation = FormationMother.With(modules: [existing]);
        FormationModuleAdder.Command command = FormationModuleAdderCommandMother.With(formation.Id.Value);
        this.ShouldFindFormation(formation);

        await this._handler.Handle(command, CancellationToken.None);

        formation.Modules.Should().HaveCount(2);
        formation.Modules.First().Should().Be(existing);
        this.ShouldHaveSavedFormation(formation);
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_module_already_exists()
    {
        FormationModule existing = FormationModuleMother.Random();
        Formation formation = FormationMother.With(modules: [existing]);
        FormationModuleAdder.Command command = FormationModuleAdderCommandMother.With(
            formation.Id.Value,
            existing.Id.Value);
        this.ShouldFindFormation(formation);

        Func<Task> act = async () => await this._handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FormationModuleAlreadyExistsError>();
        formation.Modules.Should().ContainSingle().Which.Should().Be(existing);
        this.ShouldNotHaveSavedAnyFormation();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_formation_does_not_exist()
    {
        this.ShouldNotFindFormation();

        Func<Task> act = async () =>
            await this._handler.Handle(FormationModuleAdderCommandMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<FormationNotFoundError>();
        this.ShouldNotHaveSavedAnyFormation();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_formation_is_archived()
    {
        Formation formation = FormationMother.Archived();
        this.ShouldFindFormation(formation);

        Func<Task> act = async () =>
            await this._handler.Handle(FormationModuleAdderCommandMother.With(formation.Id.Value), CancellationToken.None);

        await act.Should().ThrowAsync<FormationArchivedError>();
        this.ShouldNotHaveSavedAnyFormation();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_saving_anything_when_an_exam_has_no_questions()
    {
        Formation formation = FormationMother.Random();
        this.ShouldFindFormation(formation);

        Func<Task> act = async () => await this._handler.Handle(
            CommandOf(formation, ModuleType.Exam, quiz: null, passingScore: 70),
            CancellationToken.None);

        await act.Should().ThrowAsync<FormationExamQuizRequiredError>();
        this.ShouldNotHaveSavedAnyFormation();
        this.ShouldNotHavePublishedAnything();
    }

    [TestCase(null)]
    [TestCase(0)]
    [TestCase(101)]
    public async Task fail_without_saving_anything_when_the_exam_passing_score_is_missing_or_out_of_range(int? passingScore)
    {
        Formation formation = FormationMother.Random();
        this.ShouldFindFormation(formation);

        Func<Task> act = async () => await this._handler.Handle(
            CommandOf(formation, ModuleType.Exam, quiz: [ValidQuestion()], passingScore: passingScore),
            CancellationToken.None);

        await act.Should().ThrowAsync<FormationExamPassingScoreInvalidError>();
        this.ShouldNotHaveSavedAnyFormation();
        this.ShouldNotHavePublishedAnything();
    }

    [TestCase(1)]
    [TestCase(100)]
    public async Task accept_an_exam_with_a_passing_score_at_the_limits(int passingScore)
    {
        Formation formation = FormationMother.Random();
        this.ShouldFindFormation(formation);

        await this._handler.Handle(
            CommandOf(formation, ModuleType.Exam, quiz: [ValidQuestion()], passingScore: passingScore),
            CancellationToken.None);

        formation.Modules.Should().ContainSingle(m => m.Type == ModuleType.Exam).Which.PassingScore.Should().Be(passingScore);
    }

    [TestCase(ModuleType.Video)]
    [TestCase(ModuleType.Document)]
    [TestCase(ModuleType.Exercise)]
    public async Task fail_without_saving_anything_when_a_module_that_is_not_an_exam_carries_exam_data(ModuleType type)
    {
        Formation formation = FormationMother.Random();
        this.ShouldFindFormation(formation);

        Func<Task> act = async () => await this._handler.Handle(
            CommandOf(formation, type, quiz: null, passingScore: 70),
            CancellationToken.None);

        await act.Should().ThrowAsync<FormationExamDataOnNonExamModuleError>();
        this.ShouldNotHaveSavedAnyFormation();
        this.ShouldNotHavePublishedAnything();
    }

    private static FormationModuleAdder.QuizQuestionInput ValidQuestion() => new(WordMother.Random(), [WordMother.Random(), WordMother.Random()], 0);

    private static FormationModuleAdder.Command CommandOf(
        Formation formation,
        ModuleType type,
        IReadOnlyList<FormationModuleAdder.QuizQuestionInput>? quiz,
        int? passingScore) =>
        FormationModuleAdderCommandMother.With(formation.Id.Value, type: type, quiz: quiz, passingScore: passingScore);
}
