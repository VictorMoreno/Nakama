using Nakama.Formations.Domain.Events;
using Nakama.FormationsList.Domain;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationsList.Application.Handlers;

public sealed class FormationSummaryCreatedOnFormationCreated(IFormationSummaryRepository formationSummaryRepository)
    : DomainEventHandler<FormationCreated>
{
    protected override Task Handle(FormationCreated formationCreated, CancellationToken cancellationToken) =>
        formationSummaryRepository.Save(
            FormationSummary.Create(
                formationCreated.Id.Value,
                formationCreated.Title,
                formationCreated.Audience,
                formationCreated.Description,
                formationCreated.Status.ToString()),
            cancellationToken);
}
