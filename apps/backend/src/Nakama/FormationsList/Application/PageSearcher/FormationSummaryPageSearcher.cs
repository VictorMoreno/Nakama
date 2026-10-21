using Microsoft.Extensions.Options;
using Nakama.FormationEnrollment.Domain;
using Nakama.FormationsList.Domain;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Pagination;
using Nakama.Shared.Infrastructure.Storage;

namespace Nakama.FormationsList.Application.PageSearcher;

public static class FormationSummaryPageSearcher
{
    public sealed record Query(Guid UserId, bool IsAdmin, int Page, int? PageSize, string? Search, string? Status)
        : IQuery<PagedResponse<FormationSummaryResponse>>;

    public sealed class Handler(Searcher searcher, IOptions<PaginationOptions> pagination)
        : IQueryHandler<Query, PagedResponse<FormationSummaryResponse>>
    {
        public async Task<PagedResponse<FormationSummaryResponse>> Handle(Query query, CancellationToken ct)
        {
            PageRequest pageRequest = PageRequest.From(
                query.Page,
                query.PageSize,
                pagination.Value.DefaultPageSize,
                pagination.Value.MaxPageSize);

            return await searcher.Execute(query, pageRequest, ct);
        }
    }

    public sealed class Searcher(
        IFormationSummaryRepository summaryRepository,
        IFormationEnrollmentRepository enrollmentRepository,
        IOptions<StorageOptions> storage) : IService
    {
        public async Task<PagedResponse<FormationSummaryResponse>> Execute(
            Query query,
            PageRequest pageRequest,
            CancellationToken cancellationToken)
        {
            IReadOnlyList<FormationEnrollment.Domain.FormationEnrollment> enrollments = await enrollmentRepository.Search(
                new FormationEnrollmentByUserSpecification(query.UserId), cancellationToken);
            Dictionary<Guid, FormationEnrollment.Domain.FormationEnrollment> enrollmentsByFormation = enrollments.ToDictionary(e => e.FormationId);
            IReadOnlyCollection<Guid>? formationIds = query.IsAdmin ? null : enrollmentsByFormation.Keys.ToList();

            PagedResult<FormationSummary> result = await summaryRepository.SearchPage(
                new FormationSummaryPageSpecification(
                    formationIds,
                    query.Search,
                    query.Status,
                    pageRequest.Skip,
                    pageRequest.PageSize),
                cancellationToken);

            List<FormationSummaryResponse> items = result.Items.Select(f =>
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

            return PagedResponse<FormationSummaryResponse>.From(items, result.TotalCount, pageRequest);
        }
    }
}
