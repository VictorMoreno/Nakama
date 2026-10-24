using AwesomeAssertions;
using Nakama.Formations.Application.ModuleUpdater;
using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Nakama.Formations.Domain.Events;
using Test.Modules.Formations.Domain;
using Test.Modules.Shared;

namespace Test.Modules.Formations.Application;

[TestFixture]
public sealed class FormationModuleUpdaterShould : FormationsUnitTestCase
{
    private FormationModuleUpdater.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationModuleUpdater.Handler(new(this.FormationsRepository.Object, this.EventBus.Object));
    }

    [Test]
    public async Task replace_the_module_data_and_publish_formation_module_updated()
    {
        FormationModule module = FormationModuleMother.Random();
        Formation formation = FormationMother.With(modules: [module]);
        FormationModuleUpdater.Command command = FormationModuleUpdaterCommandMother.With(
            formation.Id.Value,
            module.Id.Value,
            type: ModuleType.Exercise,
            gated: true,
            aiDraft: true,
            resourceUrl: WordMother.Random(),
            durationMin: 30,
            passingScore: null);
        this.ShouldFindFormation(formation);

        await this._handler.Handle(command, CancellationToken.None);

        formation.Modules.Should().ContainSingle().Which.Should().Be(new FormationModule(
            module.Id,
            command.Title,
            ModuleType.Exercise,
            command.Summary,
            true,
            true,
            command.ResourceUrl,
            30,
            null,
            null));
        this.ShouldHaveSavedFormation(formation);
        this.ShouldHavePublished<FormationModuleUpdated>(@event => @event.Id == formation.Id && @event.Module.Id == module.Id);
    }

    [Test]
    public async Task update_only_the_targeted_module_and_keep_the_order()
    {
        FormationModule first = FormationModuleMother.Random();
        FormationModule target = FormationModuleMother.Random();
        FormationModule last = FormationModuleMother.Random();
        Formation formation = FormationMother.With(modules: [first, target, last]);
        FormationModuleUpdater.Command command = FormationModuleUpdaterCommandMother.With(formation.Id.Value, target.Id.Value);
        this.ShouldFindFormation(formation);

        await this._handler.Handle(command, CancellationToken.None);

        formation.Modules.Select(module => module.Id).Should().Equal(first.Id, target.Id, last.Id);
        formation.Modules.First().Should().Be(first);
        formation.Modules.Last().Should().Be(last);
        formation.Modules.ElementAt(1).Title.Should().Be(command.Title);
    }

    [Test]
    public async Task map_the_quiz_questions_of_the_command_into_the_module()
    {
        string question = WordMother.Random();
        string firstOption = WordMother.Random();
        string secondOption = WordMother.Random();
        FormationModule module = FormationModuleMother.Random();
        Formation formation = FormationMother.With(modules: [module]);
        FormationModuleUpdater.Command command = FormationModuleUpdaterCommandMother.With(
            formation.Id.Value,
            module.Id.Value,
            type: ModuleType.Exam,
            quiz: [new FormationModuleUpdater.QuizQuestionInput(question, [firstOption, secondOption], 0)],
            passingScore: 80);
        this.ShouldFindFormation(formation);

        await this._handler.Handle(command, CancellationToken.None);

        formation.Modules.Should().ContainSingle().Which.Quiz.Should().BeEquivalentTo(
            new[] { new QuizQuestion(question, [firstOption, secondOption], 0) });
        formation.Modules.Should().ContainSingle().Which.PassingScore.Should().Be(80);
    }

    [Test]
    public async Task remove_the_quiz_when_the_command_has_none()
    {
        FormationModule module = FormationModuleMother.With(
            type: ModuleType.Exam,
            quiz: [new QuizQuestion(WordMother.Random(), [WordMother.Random(), WordMother.Random()], 0)]);
        Formation formation = FormationMother.With(modules: [module]);
        this.ShouldFindFormation(formation);

        await this._handler.Handle(
            FormationModuleUpdaterCommandMother.With(formation.Id.Value, module.Id.Value, quiz: null),
            CancellationToken.None);

        formation.Modules.Should().ContainSingle().Which.Quiz.Should().BeNull();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_module_does_not_exist()
    {
        Formation formation = FormationMother.With(modules: [FormationModuleMother.Random()]);
        this.ShouldFindFormation(formation);

        Func<Task> act = async () => await this._handler.Handle(
            FormationModuleUpdaterCommandMother.With(formation.Id.Value),
            CancellationToken.None);

        await act.Should().ThrowAsync<FormationModuleNotFoundError>();
        this.ShouldNotHaveSavedAnyFormation();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_formation_does_not_exist()
    {
        this.ShouldNotFindFormation();

        Func<Task> act = async () =>
            await this._handler.Handle(FormationModuleUpdaterCommandMother.Random(), CancellationToken.None);

        await act.Should().ThrowAsync<FormationNotFoundError>();
        this.ShouldNotHaveSavedAnyFormation();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_saving_anything_when_an_exam_has_no_questions()
    {
        Formation formation = FormationMother.With(modules: [FormationModuleMother.Random()]);
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
        Formation formation = FormationMother.With(modules: [FormationModuleMother.Random()]);
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
        Formation formation = FormationMother.With(modules: [FormationModuleMother.Random()]);
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
        Formation formation = FormationMother.With(modules: [FormationModuleMother.Random()]);
        this.ShouldFindFormation(formation);

        Func<Task> act = async () => await this._handler.Handle(
            CommandOf(formation, type, quiz: null, passingScore: 70),
            CancellationToken.None);

        await act.Should().ThrowAsync<FormationExamDataOnNonExamModuleError>();
        this.ShouldNotHaveSavedAnyFormation();
        this.ShouldNotHavePublishedAnything();
    }

    private static FormationModuleUpdater.QuizQuestionInput ValidQuestion() => new(WordMother.Random(), [WordMother.Random(), WordMother.Random()], 0);

    private static FormationModuleUpdater.Command CommandOf(
        Formation formation,
        ModuleType type,
        IReadOnlyList<FormationModuleUpdater.QuizQuestionInput>? quiz,
        int? passingScore) =>
        FormationModuleUpdaterCommandMother.With(formation.Id.Value, formation.Modules.First().Id.Value, type: type, quiz: quiz, passingScore: passingScore);
}
