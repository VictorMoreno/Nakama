using AwesomeAssertions;
using Nakama.FormationDetail.Application.Handlers;
using Nakama.FormationDetail.Domain;
using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;
using Test.Modules.FormationDetail.Domain;
using Test.Modules.Formations.Domain;
using FormationDetailEntity = Nakama.FormationDetail.Domain.FormationDetail;

namespace Test.Modules.FormationDetail.Application;

[TestFixture]
public sealed class FormationDetailUpdatedOnFormationModuleUpdatedShould : FormationDetailUnitTestCase
{
    private FormationDetailUpdatedOnFormationModuleUpdated _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationDetailUpdatedOnFormationModuleUpdated(this.DetailRepository.Object);
    }

    [Test]
    public async Task replace_every_field_of_the_matching_module_row()
    {
        FormationModuleRow row = FormationModuleRowMother.With(type: ModuleType.Document.ToString(), gated: false, aiDraft: true);
        FormationModuleRow other = FormationModuleRowMother.Random();
        FormationDetailEntity detail = FormationDetailMother.With(modules: [row, other]);
        FormationModule module = FormationModuleMother.With(
            id: ModuleId.From(row.Id),
            type: ModuleType.Exam,
            gated: true,
            aiDraft: false,
            resourceUrl: "https://example.com/resource",
            durationMin: 30,
            passingScore: 70,
            quiz: [QuizQuestionMother.With(correctIndex: 1)]);
        FormationModuleUpdated @event = FormationModuleUpdatedMother.With(FormationId.From(detail.Id), module);
        this.ShouldFindFormationDetail(detail);
        string otherTitle = other.Title;

        await this._handler.Handle(new DomainEventNotification<FormationModuleUpdated>(@event), CancellationToken.None);

        row.Title.Should().Be(module.Title);
        row.Type.Should().Be(ModuleType.Exam.ToString());
        row.Summary.Should().Be(module.Summary);
        row.Gated.Should().BeTrue();
        row.AiDraft.Should().BeFalse();
        row.ResourceUrl.Should().Be(module.ResourceUrl);
        row.DurationMin.Should().Be(30);
        row.PassingScore.Should().Be(70);
        row.Quiz.Should().BeEquivalentTo(module.Quiz);
        other.Title.Should().Be(otherTitle);
        this.ShouldHaveUpdatedFormationDetail(detail);
    }

    [Test]
    public async Task remove_the_quiz_of_the_row_when_the_module_has_none()
    {
        FormationModuleRow row = FormationModuleRowMother.With(quiz: [new QuizQuestionRow()]);
        FormationDetailEntity detail = FormationDetailMother.With(modules: [row]);
        FormationModuleUpdated @event = FormationModuleUpdatedMother.With(
            FormationId.From(detail.Id),
            FormationModuleMother.With(id: ModuleId.From(row.Id), quiz: null));
        this.ShouldFindFormationDetail(detail);

        await this._handler.Handle(new DomainEventNotification<FormationModuleUpdated>(@event), CancellationToken.None);

        row.Quiz.Should().BeNull();
    }

    [Test]
    public async Task leave_the_modules_untouched_when_the_module_is_not_in_the_detail()
    {
        FormationModuleRow row = FormationModuleRowMother.Random();
        string title = row.Title;
        FormationDetailEntity detail = FormationDetailMother.With(modules: [row]);
        FormationModuleUpdated @event = FormationModuleUpdatedMother.With(FormationId.From(detail.Id));
        this.ShouldFindFormationDetail(detail);

        await this._handler.Handle(new DomainEventNotification<FormationModuleUpdated>(@event), CancellationToken.None);

        detail.Modules.Should().ContainSingle().Which.Title.Should().Be(title);
    }

    [Test]
    public async Task do_nothing_when_the_detail_does_not_exist()
    {
        FormationModuleUpdated @event = FormationModuleUpdatedMother.Random();
        this.ShouldNotFindFormationDetail();

        await this._handler.Handle(new DomainEventNotification<FormationModuleUpdated>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyFormationDetail();
    }
}
