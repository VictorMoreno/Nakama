using Nakama.FormationDetail.Domain;
using Nakama.Formations.Domain.Events;
using Nakama.Shared.Infrastructure.Events;

namespace Nakama.FormationDetail.Application.Handlers;

public sealed class FormationDetailUpdatedOnFormationModuleAdded(IFormationDetailRepository formationDetailRepository)
    : DomainEventHandler<FormationModuleAdded>
{
    protected override async Task Handle(FormationModuleAdded formationModuleAdded, CancellationToken cancellationToken)
    {
        var formationDetail = await formationDetailRepository.FindById(formationModuleAdded.Id.Value, cancellationToken);
        if (formationDetail is null)
        {
            return;
        }

        var quiz = formationModuleAdded.Module.Quiz?.Select(q => new QuizQuestionRow
        {
            Question = q.Question,
            Options = [.. q.Options],
            CorrectIndex = q.CorrectIndex,
        }).ToList();

        formationDetail.AddModule(
            formationModuleAdded.Module.Id.Value,
            formationModuleAdded.Module.Title,
            formationModuleAdded.Module.Type.ToString(),
            formationModuleAdded.Module.Summary,
            formationModuleAdded.Module.Gated,
            formationModuleAdded.Module.AiDraft,
            formationModuleAdded.Module.ResourceUrl,
            formationModuleAdded.Module.DurationMin,
            formationModuleAdded.Module.PassingScore,
            quiz);
        await formationDetailRepository.Update(formationDetail, cancellationToken);
    }
}
