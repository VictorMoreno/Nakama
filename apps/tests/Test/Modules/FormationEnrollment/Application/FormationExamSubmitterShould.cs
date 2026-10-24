using AwesomeAssertions;
using Nakama.FormationDetail.Application;
using Nakama.FormationDetail.Domain.Errors;
using Nakama.FormationEnrollment.Application.ExamSubmitter;
using Nakama.FormationEnrollment.Domain;
using Nakama.FormationEnrollment.Domain.Errors;
using Nakama.FormationEnrollment.Domain.Events;
using Nakama.Formations.Domain.Errors;
using Test.Modules.FormationDetail.Application;
using Test.Modules.FormationEnrollment.Domain;
using Enrollment = Nakama.FormationEnrollment.Domain.FormationEnrollment;

namespace Test.Modules.FormationEnrollment.Application;

[TestFixture]
public sealed class FormationExamSubmitterShould : FormationEnrollmentUnitTestCase
{
    private FormationExamSubmitter.Handler _handler = null!;

    [SetUp]
    public void SetUp()
    {
        this._handler = new FormationExamSubmitter.Handler(
            new(this.QueryBus.Object, this.EnrollmentsRepository.Object, this.EventBus.Object, this.Clock.Object));
    }

    [Test]
    public async Task complete_the_module_with_the_score_when_the_exam_is_passed()
    {
        FormationDetailResponse detail = FormationDetailResponseMother.WithExam(70, 0, 1, 2);
        Guid moduleId = detail.Modules.Single().Id;
        Enrollment enrollment = FormationEnrollmentMother.With(formationId: detail.Id);
        DateTime now = DateTime.UtcNow;
        this.ShouldBeNow(now);
        this.ShouldFindFormationDetail(detail);
        this.ShouldFindEnrollment(enrollment);

        await this._handler.Handle(
            FormationExamSubmitterCommandMother.With(detail.Id, moduleId, enrollment.UserId, 0, 1, 2),
            CancellationToken.None);

        enrollment.Progress.CompletedModuleIds.Should().Equal(moduleId);
        enrollment.Progress.ExamScores.Should().ContainKey(moduleId).WhoseValue.Should().Be(100);
        enrollment.Progress.ExamAttempts.Should().Equal(new ExamAttempt(moduleId, 100, true, now));
        this.ShouldHaveSavedEnrollment(enrollment);
        this.ShouldHavePublished<EnrollmentExamAttempted>(@event =>
            @event.ModuleId == moduleId && @event.UserId == enrollment.UserId && @event.Score == 100 && @event.Passed && @event.AttemptedAt == now);
        this.ShouldHavePublished<EnrollmentModuleCompleted>(@event =>
            @event.ModuleId == moduleId && @event.Score == 100 && @event.CompletedModuleCount == 1);
    }

    [Test]
    public async Task record_the_failed_attempt_without_completing_the_module()
    {
        FormationDetailResponse detail = FormationDetailResponseMother.WithExam(70, 0, 1, 2);
        Guid moduleId = detail.Modules.Single().Id;
        Enrollment enrollment = FormationEnrollmentMother.With(formationId: detail.Id);
        DateTime now = DateTime.UtcNow;
        this.ShouldBeNow(now);
        this.ShouldFindFormationDetail(detail);
        this.ShouldFindEnrollment(enrollment);

        await this._handler.Handle(
            FormationExamSubmitterCommandMother.With(detail.Id, moduleId, enrollment.UserId, 0, 0, 0),
            CancellationToken.None);

        enrollment.Progress.CompletedModuleIds.Should().BeEmpty();
        enrollment.Progress.ExamScores.Should().BeEmpty();
        enrollment.Progress.ExamAttempts.Should().Equal(new ExamAttempt(moduleId, 33, false, now));
        this.ShouldHaveSavedEnrollment(enrollment);
        this.ShouldHavePublished<EnrollmentExamAttempted>(@event => @event.ModuleId == moduleId && @event.Score == 33 && !@event.Passed);
        enrollment.DomainEvents.OfType<EnrollmentModuleCompleted>().Should().BeEmpty();
    }

    [Test]
    public async Task pass_when_the_score_equals_the_passing_score()
    {
        FormationDetailResponse detail = FormationDetailResponseMother.WithExam(67, 0, 1, 2);
        Guid moduleId = detail.Modules.Single().Id;
        Enrollment enrollment = FormationEnrollmentMother.With(formationId: detail.Id);
        this.ShouldBeNow(DateTime.UtcNow);
        this.ShouldFindFormationDetail(detail);
        this.ShouldFindEnrollment(enrollment);

        await this._handler.Handle(
            FormationExamSubmitterCommandMother.With(detail.Id, moduleId, enrollment.UserId, 0, 1, 0),
            CancellationToken.None);

        enrollment.Progress.CompletedModuleIds.Should().Equal(moduleId);
        enrollment.Progress.ExamScores.Should().ContainKey(moduleId).WhoseValue.Should().Be(67);
    }

    [Test]
    public async Task keep_the_earlier_failed_attempts_when_a_retry_passes()
    {
        FormationDetailResponse detail = FormationDetailResponseMother.WithExam(70, 0, 1, 2);
        Guid moduleId = detail.Modules.Single().Id;
        ExamAttempt failed = new(moduleId, 33, false, DateTime.UtcNow.AddHours(-1));
        Enrollment enrollment = FormationEnrollmentMother.With(
            formationId: detail.Id,
            progress: LearnerProgressMother.With(examAttempts: [failed]));
        DateTime now = DateTime.UtcNow;
        this.ShouldBeNow(now);
        this.ShouldFindFormationDetail(detail);
        this.ShouldFindEnrollment(enrollment);

        await this._handler.Handle(
            FormationExamSubmitterCommandMother.With(detail.Id, moduleId, enrollment.UserId, 0, 1, 2),
            CancellationToken.None);

        enrollment.Progress.ExamAttempts.Should().Equal(failed, new ExamAttempt(moduleId, 100, true, now));
        enrollment.Progress.CompletedModuleIds.Should().Equal(moduleId);
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_exam_is_already_passed()
    {
        FormationDetailResponse detail = FormationDetailResponseMother.WithExam(70, 0, 1, 2);
        Guid moduleId = detail.Modules.Single().Id;
        Enrollment enrollment = FormationEnrollmentMother.With(
            formationId: detail.Id,
            progress: LearnerProgressMother.With(completedModuleIds: [moduleId]));
        this.ShouldFindFormationDetail(detail);
        this.ShouldFindEnrollment(enrollment);

        Func<Task> act = async () => await this._handler.Handle(
            FormationExamSubmitterCommandMother.With(detail.Id, moduleId, enrollment.UserId, 0, 1, 2),
            CancellationToken.None);

        await act.Should().ThrowAsync<FormationEnrollmentExamAlreadyPassedError>();
        enrollment.Progress.ExamAttempts.Should().BeEmpty();
        this.ShouldNotHaveSavedAnyEnrollment();
        this.ShouldNotHavePublishedAnything();
    }

    [TestCase(new[] { 0, 1 })]
    [TestCase(new[] { 0, 1, 2, 0 })]
    [TestCase(new[] { 0, 1, 3 })]
    [TestCase(new[] { -1, 1, 2 })]
    public async Task fail_without_saving_anything_when_the_answers_do_not_fit_the_exam(int[] answers)
    {
        FormationDetailResponse detail = FormationDetailResponseMother.WithExam(70, 0, 1, 2);
        Guid moduleId = detail.Modules.Single().Id;
        Enrollment enrollment = FormationEnrollmentMother.With(formationId: detail.Id);
        this.ShouldFindFormationDetail(detail);
        this.ShouldFindEnrollment(enrollment);

        Func<Task> act = async () => await this._handler.Handle(
            FormationExamSubmitterCommandMother.With(detail.Id, moduleId, enrollment.UserId, answers),
            CancellationToken.None);

        await act.Should().ThrowAsync<FormationEnrollmentInvalidExamAnswersError>();
        enrollment.Progress.ExamAttempts.Should().BeEmpty();
        this.ShouldNotHaveSavedAnyEnrollment();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_module_is_not_an_exam()
    {
        FormationDetailResponse detail = FormationDetailResponseMother.WithModule();
        Guid moduleId = detail.Modules.Single().Id;
        Enrollment enrollment = FormationEnrollmentMother.With(formationId: detail.Id);
        this.ShouldFindFormationDetail(detail);
        this.ShouldFindEnrollment(enrollment);

        Func<Task> act = async () => await this._handler.Handle(
            FormationExamSubmitterCommandMother.With(detail.Id, moduleId, enrollment.UserId, 0),
            CancellationToken.None);

        await act.Should().ThrowAsync<FormationEnrollmentModuleIsNotAnExamError>();
        this.ShouldNotHaveSavedAnyEnrollment();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_previous_gated_module_is_not_completed()
    {
        FormationDetailResponse detail = FormationDetailResponseMother.With(
            modules: [FormationModuleResponseMother.With(gated: true), FormationModuleResponseMother.Exam(70, 0)]);
        Guid moduleId = detail.Modules[1].Id;
        Enrollment enrollment = FormationEnrollmentMother.With(formationId: detail.Id);
        this.ShouldFindFormationDetail(detail);
        this.ShouldFindEnrollment(enrollment);

        Func<Task> act = async () => await this._handler.Handle(
            FormationExamSubmitterCommandMother.With(detail.Id, moduleId, enrollment.UserId, 0),
            CancellationToken.None);

        await act.Should().ThrowAsync<FormationModuleLockedByGatingError>();
        this.ShouldNotHaveSavedAnyEnrollment();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_formation_is_archived()
    {
        FormationDetailResponse detail = FormationDetailResponseMother.With(
            archived: true,
            modules: [FormationModuleResponseMother.Exam(70, 0)]);
        Enrollment enrollment = FormationEnrollmentMother.With(formationId: detail.Id);
        this.ShouldFindFormationDetail(detail);
        this.ShouldFindEnrollment(enrollment);

        Func<Task> act = async () => await this._handler.Handle(
            FormationExamSubmitterCommandMother.With(detail.Id, detail.Modules.Single().Id, enrollment.UserId, 0),
            CancellationToken.None);

        await act.Should().ThrowAsync<FormationArchivedError>();
        this.ShouldNotHaveSavedAnyEnrollment();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_module_does_not_belong_to_the_formation()
    {
        FormationDetailResponse detail = FormationDetailResponseMother.WithExam(70, 0);
        Enrollment enrollment = FormationEnrollmentMother.With(formationId: detail.Id);
        this.ShouldFindFormationDetail(detail);
        this.ShouldFindEnrollment(enrollment);

        Func<Task> act = async () => await this._handler.Handle(
            FormationExamSubmitterCommandMother.With(detail.Id, Guid.NewGuid(), enrollment.UserId, 0),
            CancellationToken.None);

        await act.Should().ThrowAsync<FormationModuleNotFoundError>();
        this.ShouldNotHaveSavedAnyEnrollment();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_user_is_not_enrolled()
    {
        FormationDetailResponse detail = FormationDetailResponseMother.WithExam(70, 0);
        this.ShouldFindFormationDetail(detail);
        this.ShouldNotFindEnrollment();

        Func<Task> act = async () => await this._handler.Handle(
            FormationExamSubmitterCommandMother.With(detail.Id, detail.Modules.Single().Id, Guid.NewGuid(), 0),
            CancellationToken.None);

        await act.Should().ThrowAsync<FormationEnrollmentNotFoundError>();
        this.ShouldNotHaveSavedAnyEnrollment();
        this.ShouldNotHavePublishedAnything();
    }

    [Test]
    public async Task fail_without_saving_anything_when_the_formation_does_not_exist()
    {
        Guid formationId = Guid.NewGuid();
        this.ShouldNotFindFormationDetail(formationId);

        Func<Task> act = async () => await this._handler.Handle(
            FormationExamSubmitterCommandMother.With(formationId, Guid.NewGuid(), Guid.NewGuid(), 0),
            CancellationToken.None);

        await act.Should().ThrowAsync<FormationDetailNotFoundError>();
        this.ShouldNotHaveSavedAnyEnrollment();
        this.ShouldNotHavePublishedAnything();
    }
}
