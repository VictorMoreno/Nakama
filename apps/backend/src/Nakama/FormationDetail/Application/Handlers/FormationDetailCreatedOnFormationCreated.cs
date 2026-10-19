using Nakama.FormationDetail.Domain;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationDetail.Application.Handlers;

using FormationDetail = Nakama.FormationDetail.Domain.FormationDetail;

public sealed class FormationDetailCreatedOnFormationCreated(IFormationDetailRepository formationDetailRepository)
    : DomainEventHandler<FormationCreated>
{
    protected override Task Handle(FormationCreated formationCreated, CancellationToken cancellationToken) =>
        formationDetailRepository.Save(
            FormationDetail.Create(
                formationCreated.Id.Value,
                formationCreated.Title,
                formationCreated.Audience,
                formationCreated.Description,
                formationCreated.Status.ToString()),
            cancellationToken);
}
