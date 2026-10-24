using Nakama.FormationDetail.Application;
using Nakama.FormationDetail.Application.Finder;
using Nakama.FormationEnrollment.Domain;
using Nakama.FormationEnrollment.Domain.Errors;
using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Nakama.Shared.Domain;

namespace Nakama.FormationEnrollment.Application.LearnerModuleAccess;

public sealed record AccessibleModule(Domain.FormationEnrollment Enrollment, FormationModuleResponse Module);

public sealed class LearnerModuleAccessFinder(IQueryBus queryBus, IFormationEnrollmentRepository repository)
{
    public async Task<AccessibleModule> Execute(FormationId id, Guid moduleId, Guid userId, CancellationToken cancellationToken)
    {
        FormationDetailResponse detail = await queryBus.Ask(new FormationDetailFinder.Query(id.Value), cancellationToken);

        if (detail.Archived)
        {
            throw FormationErrors.Archived(id);
        }

        if (detail.Status != FormationStatus.Active.ToString())
        {
            throw FormationErrors.NotActive(id);
        }

        IReadOnlyList<Domain.FormationEnrollment> found = await repository.Search(
            new FormationEnrollmentByFormationAndUserSpecification(id.Value, userId),
            cancellationToken);
        Domain.FormationEnrollment enrollment = found.FirstOrDefault()
                                                ?? throw FormationEnrollmentErrors.NotEnrolled(id.Value, userId);

        List<FormationModuleResponse> modules = detail.Modules;
        int index = modules.FindIndex(m => m.Id == moduleId);
        if (index < 0)
        {
            throw FormationErrors.ModuleNotFound(moduleId.ToString(), id.Value.ToString());
        }

        if (index > 0)
        {
            FormationModuleResponse previous = modules[index - 1];
            if (previous.Gated && !enrollment.Progress.CompletedModuleIds.Contains(previous.Id))
            {
                throw FormationErrors.ModuleLockedByGating;
            }
        }

        return new AccessibleModule(enrollment, modules[index]);
    }
}
