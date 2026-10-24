using Nakama.FormationEnrollment.Application.LearnerModuleAccess;
using Nakama.FormationEnrollment.Domain;
using Nakama.FormationEnrollment.Domain.Errors;
using Nakama.Formations.Domain;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationEnrollment.Application.ExamSubmitter;

public static class FormationExamSubmitter
{
    public sealed record Command(Guid Id, Guid ModuleId, Guid UserId, IReadOnlyList<int> Answers) : ICommand;

    public sealed class Handler(Submitter submitter) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await submitter.Execute(FormationId.From(cmd.Id), cmd.ModuleId, cmd.UserId, cmd.Answers, ct);
        }
    }

    public sealed class Submitter(
        IQueryBus queryBus,
        IFormationEnrollmentRepository enrollmentRepository,
        IEventBus eventBus,
        ISystemClock systemClock) : IService
    {
        private readonly LearnerModuleAccessFinder _access = new(queryBus, enrollmentRepository);

        public async Task Execute(
            FormationId id,
            Guid moduleId,
            Guid userId,
            IReadOnlyList<int> answers,
            CancellationToken cancellationToken)
        {
            AccessibleModule access = await this._access.Execute(id, moduleId, userId, cancellationToken);

            if (access.Module.Type != ModuleType.Exam.ToString()
                || access.Module.PassingScore is null
                || access.Module.Quiz is null)
            {
                throw FormationEnrollmentErrors.ModuleIsNotAnExam(moduleId);
            }

            List<ExamAnswerKey> key = access.Module.Quiz
                .Select(question => new ExamAnswerKey(
                    question.Options.Count,
                    question.CorrectIndex ?? throw FormationEnrollmentErrors.ModuleIsNotAnExam(moduleId)))
                .ToList();
            int score = ExamGrader.Score(answers, key);

            access.Enrollment.SubmitExam(moduleId, score, score >= access.Module.PassingScore, systemClock);

            await enrollmentRepository.Save(access.Enrollment, cancellationToken);
            await eventBus.Publish(access.Enrollment, cancellationToken);
        }
    }
}
