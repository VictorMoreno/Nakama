using Nakama.FormationEnrollment.Application.LearnerModuleAccess;
using Nakama.FormationEnrollment.Domain;
using Nakama.FormationEnrollment.Domain.Errors;
using Nakama.Formations.Domain;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationEnrollment.Application.ModuleCompleter;

public static class FormationModuleCompleter
{
    public sealed record Command(Guid Id, Guid ModuleId, Guid UserId) : ICommand;

    public sealed class Handler(Completer completer) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await completer.Execute(FormationId.From(cmd.Id), cmd.ModuleId, cmd.UserId, ct);
        }
    }

    public sealed class Completer(
        IQueryBus queryBus,
        IFormationEnrollmentRepository enrollmentRepository,
        IEventBus eventBus) : IService
    {
        private readonly LearnerModuleAccessFinder _access = new(queryBus, enrollmentRepository);

        public async Task Execute(FormationId id, Guid moduleId, Guid userId, CancellationToken cancellationToken)
        {
            AccessibleModule access = await this._access.Execute(id, moduleId, userId, cancellationToken);

            if (access.Module.Type == ModuleType.Exam.ToString())
            {
                throw FormationEnrollmentErrors.ExamMustBeSubmitted(moduleId);
            }

            access.Enrollment.CompleteModule(moduleId);

            await enrollmentRepository.Save(access.Enrollment, cancellationToken);
            await eventBus.Publish(access.Enrollment, cancellationToken);
        }
    }
}
