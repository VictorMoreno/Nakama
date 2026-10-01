using Nakama.FormationEnrollment.Domain;
using Nakama.FormationsList.Domain;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;

namespace Nakama.FormationsList.Application.Searcher;

public static class FormationSummarySearcher
{
    public sealed record Query(Guid UserId, bool IsAdmin) : IQuery<IReadOnlyCollection<FormationSummaryResponse>>;

    public sealed class Handler(Searcher searcher)
        : IQueryHandler<Query, IReadOnlyCollection<FormationSummaryResponse>>
    {
        public async Task<IReadOnlyCollection<FormationSummaryResponse>> Handle(Query query, CancellationToken ct) =>
            await searcher.Execute(query.UserId, query.IsAdmin, ct);
    }

    // An admin sees every formation (they manage all of them); anyone else only sees the ones
    // they're assigned to - "solo las personas asignadas pueden realizar una formación" starts
    // right here, at the listing, not just at the execute page.
    public sealed class Searcher(IFormationSummaryRepository summaryRepository, IFormationEnrollmentRepository enrollmentRepository) : IService
    {
        public async Task<IReadOnlyCollection<FormationSummaryResponse>> Execute(Guid userId, bool isAdmin, CancellationToken cancellationToken)
        {
            var enrollments = await enrollmentRepository.FindAllForUser(userId, cancellationToken);
            var enrollmentsByFormation = enrollments.ToDictionary(e => e.FormationId);

            var formations = isAdmin
                ? await summaryRepository.FindAll(cancellationToken)
                : await summaryRepository.FindByIds(enrollmentsByFormation.Keys.ToList(), cancellationToken);

            return formations.Select(f =>
            {
                var enrollment = enrollmentsByFormation.GetValueOrDefault(f.Id);
                var completedCount = enrollment?.Progress.CompletedModuleIds.Count ?? 0;
                var progressPct = f.ModulesCount == 0 ? 0 : completedCount * 100 / f.ModulesCount;

                return new FormationSummaryResponse(
                    f.Id,
                    f.Title,
                    f.Audience,
                    f.Description,
                    f.Status,
                    f.Archived,
                    f.IconUrl is null ? null : $"/api/formations/icons/{f.IconUrl}",
                    progressPct,
                    f.ModulesCount,
                    completedCount,
                    enrollment is not null);
            }).ToList();
        }
    }
}
