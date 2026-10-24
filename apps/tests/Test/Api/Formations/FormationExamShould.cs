using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Nakama.FormationDetail.Application;
using Nakama.FormationEnrollment.Application;
using Nakama.Formations.Domain;
using Nakama.Home.Application;
using Test.Api.Support;
using Test.Modules.Shared;

namespace Test.Api.Formations;

[TestFixture]
public sealed class FormationExamShould : ApiTestCase
{
    [Test]
    public async Task hide_the_correct_answers_from_a_learner_and_show_them_to_an_administrator()
    {
        (string admin, SignedInUser learner, Guid formation, Guid _) = await this.CreateFormationWithExam(passingScore: 60, correctIndexes: [1, 2]);

        FormationDetailResponse? asLearner = await (await this.Client.GetFormation(formation, learner.Token))
            .Content.ReadFromJsonAsync<FormationDetailResponse>();
        FormationDetailResponse? asAdmin = await (await this.Client.GetFormation(formation, admin))
            .Content.ReadFromJsonAsync<FormationDetailResponse>();

        asLearner!.Modules.Single().Quiz!.Select(question => question.CorrectIndex).Should().OnlyContain(correctIndex => correctIndex == null);
        asLearner.Modules.Single().Quiz!.Should().HaveCount(2);
        asAdmin!.Modules.Single().Quiz!.Select(question => question.CorrectIndex).Should().Equal(1, 2);
    }

    [Test]
    public async Task record_a_failed_attempt_let_the_learner_retry_and_complete_the_module_when_it_passes()
    {
        (_, SignedInUser learner, Guid formation, Guid module) = await this.CreateFormationWithExam(passingScore: 60, correctIndexes: [1, 2]);

        HttpResponseMessage failed = await this.Client.SubmitFormationExam(formation, module, [0, 0], learner.Token);
        FormationEnrollmentResponse afterFailure = await this.ReadEnrollment(formation, learner);
        HttpResponseMessage passed = await this.Client.SubmitFormationExam(formation, module, [1, 2], learner.Token);
        FormationEnrollmentResponse afterPass = await this.ReadEnrollment(formation, learner);

        failed.StatusCode.Should().Be(HttpStatusCode.NoContent);
        afterFailure.CompletedModuleIds.Should().BeEmpty();
        afterFailure.ExamAttempts.Should().ContainSingle().Which.Should().Match<ExamAttemptResponse>(
            attempt => attempt.ModuleId == module && attempt.Score == 0 && !attempt.Passed);
        passed.StatusCode.Should().Be(HttpStatusCode.NoContent);
        afterPass.CompletedModuleIds.Should().Equal(module);
        afterPass.ExamScores.Should().ContainKey(module).WhoseValue.Should().Be(100);
        afterPass.ExamAttempts.Select(attempt => (attempt.Score, attempt.Passed)).Should().Equal((0, false), (100, true));
    }

    [Test]
    public async Task reject_submitting_an_exam_that_is_already_passed()
    {
        (_, SignedInUser learner, Guid formation, Guid module) = await this.CreateFormationWithExam(passingScore: 60, correctIndexes: [1]);
        await this.Client.SubmitFormationExam(formation, module, [1], learner.Token);

        HttpResponseMessage again = await this.Client.SubmitFormationExam(formation, module, [0], learner.Token);

        again.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await again.Content.ReadFromJsonAsync<ProblemDetails>())!.Title.Should().Be("nakama.formation_enrollment.exam_already_passed");
        (await this.ReadEnrollment(formation, learner)).ExamAttempts.Should().ContainSingle();
    }

    [Test]
    public async Task reject_completing_an_exam_without_taking_it()
    {
        (_, SignedInUser learner, Guid formation, Guid module) = await this.CreateFormationWithExam(passingScore: 60, correctIndexes: [1]);

        HttpResponseMessage response = await this.Client.CompleteFormationModule(formation, module, learner.Token);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Title.Should().Be("nakama.formation_enrollment.exam_must_be_submitted");
        (await this.ReadEnrollment(formation, learner)).CompletedModuleIds.Should().BeEmpty();
    }

    [Test]
    public async Task reject_answers_that_do_not_fit_the_exam()
    {
        (_, SignedInUser learner, Guid formation, Guid module) = await this.CreateFormationWithExam(passingScore: 60, correctIndexes: [1, 2]);

        HttpResponseMessage response = await this.Client.SubmitFormationExam(formation, module, [1], learner.Token);

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await response.Content.ReadFromJsonAsync<ProblemDetails>())!.Title.Should().Be("nakama.formation_enrollment.invalid_exam_answers");
        (await this.ReadEnrollment(formation, learner)).ExamAttempts.Should().BeEmpty();
    }

    [Test]
    public async Task return_not_found_when_the_user_is_not_assigned_to_the_formation()
    {
        (_, _, Guid formation, Guid module) = await this.CreateFormationWithExam(passingScore: 60, correctIndexes: [1]);
        SignedInUser stranger = await this.SignInAs(isAdmin: false);

        HttpResponseMessage response = await this.Client.SubmitFormationExam(formation, module, [1], stranger.Token);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<FormationEnrollmentResponse> ReadEnrollment(Guid formation, SignedInUser learner) =>
        (await (await this.Client.GetFormationEnrollment(formation, learner.Token)).Content.ReadFromJsonAsync<FormationEnrollmentResponse>())!;

    private async Task<(string Admin, SignedInUser Learner, Guid Formation, Guid Module)> CreateFormationWithExam(
        int passingScore,
        IReadOnlyList<int> correctIndexes)
    {
        string admin = await this.SignIn(isAdmin: true);
        SignedInUser learner = await this.SignInAs(isAdmin: false);
        Guid formation = Guid.NewGuid();
        Guid module = Guid.NewGuid();
        (await this.Client.CreateFormation(formation, WordMother.Random(), WordMother.Random(), WordMother.Random(), FormationStatus.Active, admin))
            .StatusCode.Should().Be(HttpStatusCode.Created);
        (await this.Client.AddFormationExamModule(formation, module, passingScore, correctIndexes, admin)).StatusCode.Should().Be(HttpStatusCode.Created);
        (await this.Client.AddFormationTeamMember(formation, learner.Id, admin)).StatusCode.Should().Be(HttpStatusCode.NoContent);

        return (admin, learner, formation, module);
    }

    [Test]
    public async Task let_an_administrator_see_the_exam_attempts_of_everyone_in_the_formation()
    {
        (string admin, SignedInUser learner, Guid formation, Guid module) = await this.CreateFormationWithExam(passingScore: 60, correctIndexes: [1]);
        await this.Client.SubmitFormationExam(formation, module, [0], learner.Token);
        await this.Client.SubmitFormationExam(formation, module, [1], learner.Token);

        HttpResponseMessage response = await this.Client.GetFormationEnrollments(formation, admin);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        List<FormationEnrollmentResponse>? enrollments = await response.Content.ReadFromJsonAsync<List<FormationEnrollmentResponse>>();
        FormationEnrollmentResponse enrollment = enrollments.Should().ContainSingle(item => item.UserId == learner.Id).Which;
        enrollment.ExamAttempts.Select(attempt => (attempt.Score, attempt.Passed)).Should().Equal((0, false), (100, true));
        enrollment.CompletedModuleIds.Should().Equal(module);
    }

    [Test]
    public async Task forbid_a_regular_user_from_listing_the_progress_of_the_formation()
    {
        (_, SignedInUser learner, Guid formation, _) = await this.CreateFormationWithExam(passingScore: 60, correctIndexes: [1]);

        HttpResponseMessage response = await this.Client.GetFormationEnrollments(formation, learner.Token);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task summarise_the_progress_and_the_failed_exams_of_the_signed_in_person_in_the_home()
    {
        (_, SignedInUser learner, Guid formation, Guid module) = await this.CreateFormationWithExam(passingScore: 60, correctIndexes: [1, 2]);
        await this.Client.SubmitFormationExam(formation, module, [1, 0], learner.Token);

        HttpResponseMessage response = await this.Client.Get("/api/home", learner.Token);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        HomeResponse? home = await response.Content.ReadFromJsonAsync<HomeResponse>();
        HomeFormationRow row = home!.Formations.Should().ContainSingle().Subject;
        row.FormationId.Should().Be(formation);
        row.Completed.Should().BeFalse();
        row.CompletedModules.Should().Be(0);
        HomePendingExamRow exam = home.PendingExams.Should().ContainSingle().Subject;
        exam.ModuleId.Should().Be(module);
        exam.LastScore.Should().Be(50);
        exam.PassingScore.Should().Be(60);
    }
}
