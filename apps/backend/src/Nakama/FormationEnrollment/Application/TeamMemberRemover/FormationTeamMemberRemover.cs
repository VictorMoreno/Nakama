using Nakama.Formations.Domain;
using Nakama.FormationEnrollment.Domain;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationEnrollment.Application.TeamMemberRemover;

public static class FormationTeamMemberRemover
{
    public sealed record Command(Guid Id, Guid UserId) : ICommand;

    public sealed class Handler(Remover remover) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await remover.Execute(FormationId.From(cmd.Id), cmd.UserId, ct);
        }
    }

    public sealed class Remover(IFormationEnrollmentRepository enrollmentRepository, IEventBus eventBus) : IService
    {
        public async Task Execute(FormationId id, Guid userId, CancellationToken cancellationToken)
        {
            Domain.FormationEnrollment? enrollment = await enrollmentRepository.FindByFormationAndUser(id.Value, userId, cancellationToken);
            if (enrollment is null)
            {
                return;
            }

            enrollment.Unenroll();

            await enrollmentRepository.Remove(enrollment, cancellationToken);
            await eventBus.Publish(enrollment, cancellationToken);
        }
    }
}
