using Microsoft.Extensions.Options;
using Nakama.FormationEnrollment.Domain;
using Nakama.FormationsList.Domain;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Domain.Specification;
using Nakama.Shared.Infrastructure.Storage;

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

    public sealed class Searcher(
        IFormationSummaryRepository summaryRepository,
        IFormationEnrollmentRepository enrollmentRepository,
        IOptions<StorageOptions> storage) : IService
    {
        public async Task<IReadOnlyCollection<FormationSummaryResponse>> Execute(Guid userId, bool isAdmin, CancellationToken cancellationToken)
        {
            IReadOnlyList<FormationEnrollment.Domain.FormationEnrollment> enrollments = await enrollmentRepository.FindAllForUser(userId, cancellationToken);
            Dictionary<Guid, FormationEnrollment.Domain.FormationEnrollment> enrollmentsByFormation = enrollments.ToDictionary(e => e.FormationId);

            ISpecification<FormationSummary> specification = isAdmin
                ? new FormationSummaryListingSpecification()
                : new FormationSummaryByIdsSpecification(enrollmentsByFormation.Keys.ToList());
            IReadOnlyCollection<FormationSummary> formations = await summaryRepository.Search(specification, cancellationToken);

            return formations.Select(f =>
            {
                FormationEnrollment.Domain.FormationEnrollment? enrollment = enrollmentsByFormation.GetValueOrDefault(f.Id);
                int completedCount = enrollment?.Progress.CompletedModuleIds.Count ?? 0;
                int progressPct = f.ModulesCount == 0 ? 0 : completedCount * 100 / f.ModulesCount;

                return new FormationSummaryResponse(
                    f.Id,
                    f.Title,
                    f.Audience,
                    f.Description,
                    f.Status,
                    f.Archived,
                    f.IconUrl is null ? null : $"{storage.Value.FormationIconsRoute}/{f.IconUrl}",
                    progressPct,
                    f.ModulesCount,
                    completedCount,
                    enrollment is not null);
            }).ToList();
        }
    }
}
