using Nakama.Formations.Domain;
using Nakama.FormationDetail.Application.Finder;
using Nakama.FormationEnrollment.Domain;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationEnrollment.Application.TeamMemberAdder;

using FormationEnrollment = Nakama.FormationEnrollment.Domain.FormationEnrollment;

public static class FormationTeamMemberAdder
{
    public sealed record Command(Guid Id, Guid UserId) : ICommand;

    public sealed class Handler(Adder adder) : ICommandHandler<Command>
    {
        public async Task Handle(Command cmd, CancellationToken ct)
        {
            await adder.Execute(FormationId.From(cmd.Id), cmd.UserId, ct);
        }
    }

    public sealed class Adder(
        IQueryBus queryBus,
        IFormationEnrollmentRepository enrollmentRepository,
        IEventBus eventBus) : IService
    {
        public async Task Execute(FormationId id, Guid userId, CancellationToken cancellationToken)
        {
            await queryBus.Ask(new FormationDetailFinder.Query(id.Value), cancellationToken); // 404 if the formation doesn't exist

            FormationEnrollment? existing = await enrollmentRepository.FindByFormationAndUser(id.Value, userId, cancellationToken);
            if (existing is not null)
            {
                return;
            }

            FormationEnrollment enrollment = FormationEnrollment.Create(FormationEnrollmentId.From(Guid.NewGuid()), id.Value, userId);

            await enrollmentRepository.Save(enrollment, cancellationToken);
            await eventBus.Publish(enrollment, cancellationToken);
        }
    }
}
