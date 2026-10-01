using Nakama.Formations.Domain.Events;
using Nakama.FormationDetail.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationDetail.Application.Handlers;

public sealed class FormationDetailUpdatedOnFormationTeamMemberAdded(IFormationDetailRepository formationDetailRepository)
    : DomainEventHandler<FormationTeamMemberAdded>
{
    protected override async Task Handle(FormationTeamMemberAdded formationTeamMemberAdded, CancellationToken ct)
    {
        var formationDetail = await formationDetailRepository.FindById(formationTeamMemberAdded.Id.Value, ct);
        if (formationDetail is null)
        {
            return;
        }

        formationDetail.AddTeamMember(formationTeamMemberAdded.Member.UserId);
        await formationDetailRepository.Update(formationDetail, ct);
    }
}
