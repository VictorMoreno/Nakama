using AwesomeAssertions;
using Nakama.FormationDetail.Application;
using Nakama.FormationDetail.Domain.Errors;
using Nakama.FormationEnrollment.Application.ModuleCompleter;
using Nakama.FormationEnrollment.Domain.Errors;
using Nakama.FormationEnrollment.Domain.Events;
using Nakama.Formations.Domain.Errors;
using Test.Modules.FormationDetail.Application;
using Test.Modules.FormationEnrollment.Domain;
using Enrollment = Nakama.FormationEnrollment.Domain.FormationEnrollment;

namespace Test.Modules.FormationEnrollment.Application;

[TestFixture]
public sealed class FormationModuleCompleterShould : FormationEnrollmentUnitTestCase
{
    private FormationModuleCompleter.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationModuleCompleter.Handler(
            new(this.QueryBus.Object, this.EnrollmentsRepository.Object, this.EventBus.Object));
    }

    [Test]
    public async Task complete_the_module_and_publish_enrollment_module_completed()
    {
        FormationDetailResponse detail = FormationDetailResponseMother.WithModule();
        FormationModuleResponse module = detail.Modules.Single();
        Enrollment enrollment = FormationEnrollmentMother.With(formationId: detail.Id);
        FormationModuleCompleter.Command command = FormationModuleCompleterCommandMother.With(
            detail.Id,
            module.Id,
            enrollment.UserId);
        this.ShouldFindFormationDetail(detail);
        this.ShouldFindEnrollment(enrollment);

        await this._handler.Handle(command, CancellationToken.None);

        enrollment.Progress.CompletedModuleIds.Should().Equal(module.Id);
        enrollment.Progress.ExamScores.Should().BeEmpty();
        this.ShouldHaveAskedForFormationDetail(detail.Id);
        this.ShouldHaveSavedEnrollment(enrollment);
        this.ShouldHavePublished<EnrollmentModuleCompleted>(@event =>
            @event.FormationId == detail.Id
            && @event.UserId == enrollment.UserId
            && @event.ModuleId == module.Id
            && @event.Score == null
            && @event.CompletedModuleCount == 1);
    }

    [Test]
    public async Task count_every_completed_module_in_the_published_event()
    {
        FormationDetailResponse detail = FormationDetailResponseMother.WithTwoModules(firstGated: false);
        FormationModuleResponse first = detail.Modules[0];
        FormationModuleResponse second = detail.Modules[1];
        Enrollment enrollment = FormationEnrollmentMother.With(
            formationId: detail.Id,
            progress: LearnerProgressMother.With(completedModuleIds: [first.Id]));
        this.ShouldFindFormationDetail(detail);
        this.ShouldFindEnrollment(enrollment);

        await this._handler.Handle(
            FormationModuleCompleterCommandMother.With(detail.Id, second.Id, enrollment.UserId),
            CancellationToken.None);

        this.ShouldHavePublished<EnrollmentModuleCompleted>(@event => @event.CompletedModuleCount == 2);
    }

    [Test]
    public async Task complete_the_module_when_the_previous_gated_module_is_already_completed()
    {
        FormationDetailResponse detail = FormationDetailResponseMother.WithTwoModules(firstGated: true);
        FormationModuleResponse first = detail.Modules[0];
        FormationModuleResponse second = detail.Modules[1];
        Enrollment enrollment = FormationEnrollmentMother.With(
            formationId: detail.Id,
            progress: LearnerProgressMother.With(completedModuleIds: [first.Id]));
        this.ShouldFindFormationDetail(detail);
        this.ShouldFindEnrollment(enrollment);

        await this._handler.Handle(
            FormationModuleCompleterCommandMother.With(detail.Id, second.Id, enrollment.UserId),
            CancellationToken.None);

        enrollment.Progress.CompletedModuleIds.Should().Contain(second.Id);
        this.ShouldHaveSavedEnrollment(enrollment);
    }

    [Test]
    public async Task complete_the_module_when_the_previous_module_is_not_gated_even_if_it_is_not_completed()
    {
        FormationDetailResponse detail = FormationDetailResponseMother.WithTwoModules(firstGated: false);
        FormationModuleResponse second = detail.Modules[1];
        Enrollment enrollment = FormationEnrollmentMother.With(formationId: detail.Id);
        this.ShouldFindFormationDetail(detail);
        this.ShouldFindEnrollment(enrollment);

        await this._handler.Handle(
            FormationModuleCompleterCommandMother.With(detail.Id, second.Id, enrollment.UserId),
            CancellationToken.None);

        enrollment.Progress.CompletedModuleIds.Should().Equal(second.Id);
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_previous_gated_module_is_not_completed()
    {
        FormationDetailResponse detail = FormationDetailResponseMother.WithTwoModules(firstGated: true);
        FormationModuleResponse second = detail.Modules[1];
        Enrollment enrollment = FormationEnrollmentMother.With(formationId: detail.Id);
        this.ShouldFindFormationDetail(detail);
        this.ShouldFindEnrollment(enrollment);

        Func<Task> act = async () => await this._handler.Handle(
            FormationModuleCompleterCommandMother.With(detail.Id, second.Id, enrollment.UserId),
            CancellationToken.None);

        await act.Should().ThrowAsync<FormationModuleLockedByGatingError>();
        enrollment.Progress.CompletedModuleIds.Should().BeEmpty();
        this.ShouldNotHaveSavedAnyEnrollment();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_module_does_not_belong_to_the_formation()
    {
        FormationDetailResponse detail = FormationDetailResponseMother.WithModule();
        Enrollment enrollment = FormationEnrollmentMother.With(formationId: detail.Id);
        this.ShouldFindFormationDetail(detail);
        this.ShouldFindEnrollment(enrollment);

        Func<Task> act = async () => await this._handler.Handle(
            FormationModuleCompleterCommandMother.With(detail.Id, userId: enrollment.UserId),
            CancellationToken.None);

        await act.Should().ThrowAsync<FormationModuleNotFoundError>();
        this.ShouldNotHaveSavedAnyEnrollment();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_user_is_not_enrolled()
    {
        FormationDetailResponse detail = FormationDetailResponseMother.WithModule();
        FormationModuleResponse module = detail.Modules.Single();
        this.ShouldFindFormationDetail(detail);
        this.ShouldNotFindEnrollment();

        Func<Task> act = async () => await this._handler.Handle(
            FormationModuleCompleterCommandMother.With(detail.Id, module.Id),
            CancellationToken.None);

        await act.Should().ThrowAsync<FormationEnrollmentNotFoundError>();
        this.ShouldNotHaveSavedAnyEnrollment();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_formation_does_not_exist()
    {
        FormationModuleCompleter.Command command = FormationModuleCompleterCommandMother.Random();
        this.ShouldNotFindFormationDetail(command.Id);

        Func<Task> act = async () => await this._handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FormationDetailNotFoundError>();
        this.ShouldNotHaveSavedAnyEnrollment();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task keep_the_module_once_when_it_is_completed_again()
    {
        FormationDetailResponse detail = FormationDetailResponseMother.WithModule();
        FormationModuleResponse module = detail.Modules.Single();
        Enrollment enrollment = FormationEnrollmentMother.With(
            formationId: detail.Id,
            progress: LearnerProgressMother.With(completedModuleIds: [module.Id]));
        this.ShouldFindFormationDetail(detail);
        this.ShouldFindEnrollment(enrollment);

        await this._handler.Handle(
            FormationModuleCompleterCommandMother.With(detail.Id, module.Id, enrollment.UserId),
            CancellationToken.None);

        enrollment.Progress.CompletedModuleIds.Should().Equal(module.Id);
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_module_is_an_exam()
    {
        FormationDetailResponse detail = FormationDetailResponseMother.WithExam(70, 0, 1);
        Enrollment enrollment = FormationEnrollmentMother.With(formationId: detail.Id);
        this.ShouldFindFormationDetail(detail);
        this.ShouldFindEnrollment(enrollment);

        Func<Task> act = async () => await this._handler.Handle(
            FormationModuleCompleterCommandMother.With(detail.Id, detail.Modules.Single().Id, enrollment.UserId),
            CancellationToken.None);

        await act.Should().ThrowAsync<FormationEnrollmentExamMustBeSubmittedError>();
        enrollment.Progress.CompletedModuleIds.Should().BeEmpty();
        this.ShouldNotHaveSavedAnyEnrollment();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_formation_is_archived()
    {
        FormationDetailResponse detail = FormationDetailResponseMother.With(
            archived: true,
            modules: [FormationModuleResponseMother.Random()]);
        Enrollment enrollment = FormationEnrollmentMother.With(formationId: detail.Id);
        this.ShouldFindFormationDetail(detail);
        this.ShouldFindEnrollment(enrollment);

        Func<Task> act = async () => await this._handler.Handle(
            FormationModuleCompleterCommandMother.With(detail.Id, detail.Modules.Single().Id, enrollment.UserId),
            CancellationToken.None);

        await act.Should().ThrowAsync<FormationArchivedError>();
        this.ShouldNotHaveSavedAnyEnrollment();
        this.ShouldNotHavePublishedAnything();
    }

    [TestCase("Draft")]
    [TestCase("Inactive")]
    public async Task fail_without_saving_anything_when_the_formation_is_not_active(string status)
    {
        FormationDetailResponse detail = FormationDetailResponseMother.With(
            status: status,
            modules: [FormationModuleResponseMother.Random()]);
        Enrollment enrollment = FormationEnrollmentMother.With(formationId: detail.Id);
        this.ShouldFindFormationDetail(detail);
        this.ShouldFindEnrollment(enrollment);

        Func<Task> act = async () => await this._handler.Handle(
            FormationModuleCompleterCommandMother.With(detail.Id, detail.Modules.Single().Id, enrollment.UserId),
            CancellationToken.None);

        await act.Should().ThrowAsync<FormationNotActiveError>();
        enrollment.Progress.CompletedModuleIds.Should().BeEmpty();
        this.ShouldNotHaveSavedAnyEnrollment();
        this.ShouldNotHavePublishedAnything();
    }
}
