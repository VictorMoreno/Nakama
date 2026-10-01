using Nakama.Formations.Domain.Events;
using Nakama.FormationDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationDetail.Application.Handlers;

public sealed class FormationDetailUpdatedOnFormationTeamMemberRemoved(IFormationDetailRepository formationDetailRepository)
    : DomainEventHandler<FormationTeamMemberRemoved>
{
    protected override async Task Handle(FormationTeamMemberRemoved formationTeamMemberRemoved, CancellationToken ct)
    {
        var formationDetail = await formationDetailRepository.FindById(formationTeamMemberRemoved.Id.Value, ct);
        if (formationDetail is null)
        {
            return;
        }

        formationDetail.RemoveTeamMember(formationTeamMemberRemoved.Index);
        await formationDetailRepository.Update(formationDetail, ct);
    }
}
