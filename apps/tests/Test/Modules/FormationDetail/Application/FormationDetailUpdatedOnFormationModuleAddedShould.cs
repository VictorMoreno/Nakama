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
public sealed class FormationDetailUpdatedOnFormationModuleAddedShould : FormationDetailUnitTestCase
{
    private FormationDetailUpdatedOnFormationModuleAdded _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationDetailUpdatedOnFormationModuleAdded(this.DetailRepository.Object);
    }

    [Test]
    public async Task add_a_module_row_with_every_field_of_the_module()
    {
        FormationModule module = FormationModuleMother.With(
            type: ModuleType.Exam,
            gated: true,
            aiDraft: false,
            resourceUrl: "https://example.com/resource",
            durationMin: 30,
            passingScore: 70,
            quiz: [QuizQuestionMother.With(correctIndex: 1)]);
        FormationDetailEntity detail = FormationDetailMother.Random();
        FormationModuleAdded @event = FormationModuleAddedMother.With(FormationId.From(detail.Id), module);
        this.ShouldFindFormationDetail(detail);

        await this._handler.Handle(new DomainEventNotification<FormationModuleAdded>(@event), CancellationToken.None);

        FormationModuleRow row = detail.Modules.Should().ContainSingle().Which;
        row.Id.Should().Be(module.Id.Value);
        row.Title.Should().Be(module.Title);
        row.Type.Should().Be(ModuleType.Exam.ToString());
        row.Summary.Should().Be(module.Summary);
        row.Gated.Should().BeTrue();
        row.AiDraft.Should().BeFalse();
        row.ResourceUrl.Should().Be(module.ResourceUrl);
        row.DurationMin.Should().Be(30);
        row.PassingScore.Should().Be(70);
        row.Quiz.Should().BeEquivalentTo(module.Quiz);
        this.ShouldHaveUpdatedFormationDetail(detail);
    }

    [Test]
    public async Task add_the_module_without_quiz_when_it_has_none()
    {
        FormationDetailEntity detail = FormationDetailMother.Random();
        FormationModuleAdded @event = FormationModuleAddedMother.With(
            FormationId.From(detail.Id),
            FormationModuleMother.With(quiz: null));
        this.ShouldFindFormationDetail(detail);

        await this._handler.Handle(new DomainEventNotification<FormationModuleAdded>(@event), CancellationToken.None);

        detail.Modules.Should().ContainSingle().Which.Quiz.Should().BeNull();
    }

    [Test]
    public async Task keep_the_modules_already_in_the_detail()
    {
        FormationModuleRow existing = FormationModuleRowMother.Random();
        FormationDetailEntity detail = FormationDetailMother.With(modules: [existing]);
        FormationModuleAdded @event = FormationModuleAddedMother.With(FormationId.From(detail.Id));
        this.ShouldFindFormationDetail(detail);

        await this._handler.Handle(new DomainEventNotification<FormationModuleAdded>(@event), CancellationToken.None);

        detail.Modules.Should().HaveCount(2);
        detail.Modules.First().Should().BeSameAs(existing);
    }

    [Test]
    public async Task do_nothing_when_the_detail_does_not_exist()
    {
        FormationModuleAdded @event = FormationModuleAddedMother.Random();
        this.ShouldNotFindFormationDetail();

        await this._handler.Handle(new DomainEventNotification<FormationModuleAdded>(@event), CancellationToken.None);

        this.ShouldNotHaveUpdatedAnyFormationDetail();
    }
}
