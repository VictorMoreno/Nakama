using Nakama.FormationDetail.Application;
using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Nakama.FormationDetail.Application.Finder;
using Nakama.FormationDetail.Domain;
using Nakama.FormationEnrollment.Domain;
using Nakama.FormationEnrollment.Domain.Errors;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationEnrollment.Application.ModuleCompleter;

public static class FormationModuleCompleter
{
    public sealed record Command(Guid Id, Guid ModuleId, Guid UserId, int? Score) : ICommand;

    public sealed class Handler(Completer completer) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await completer.Execute(FormationId.From(cmd.Id), cmd.ModuleId, cmd.UserId, cmd.Score, ct);
        }
    }

    public sealed class Completer(
        IQueryBus queryBus,
        IFormationEnrollmentRepository enrollmentRepository,
        IEventBus eventBus) : IService
    {
        public async Task Execute(FormationId id, Guid moduleId, Guid userId, int? score,
            CancellationToken cancellationToken)
        {
            FormationDetailResponse detail = await queryBus.Ask(new FormationDetailFinder.Query(id.Value), cancellationToken);

            IReadOnlyList<Domain.FormationEnrollment> found = await enrollmentRepository.Search(
                new FormationEnrollmentByFormationAndUserSpecification(id.Value, userId),
                cancellationToken);
            Domain.FormationEnrollment enrollment = found.FirstOrDefault()
                                                    ?? throw FormationEnrollmentErrors.NotEnrolled(id.Value, userId);

            List<FormationModuleRow> modules = detail.Modules;
            int index = modules.FindIndex(m => m.Id == moduleId);
            if (index < 0)
            {
                throw FormationErrors.ModuleNotFound(moduleId.ToString(), id.Value.ToString());
            }

            if (index > 0)
            {
                FormationModuleRow previous = modules[index - 1];
                if (previous.Gated && !enrollment.Progress.CompletedModuleIds.Contains(previous.Id))
                {
                    throw FormationErrors.ModuleLockedByGating;
                }
            }

            enrollment.CompleteModule(moduleId, score);

            await enrollmentRepository.Save(enrollment, cancellationToken);
            await eventBus.Publish(enrollment, cancellationToken);
        }
    }
}
