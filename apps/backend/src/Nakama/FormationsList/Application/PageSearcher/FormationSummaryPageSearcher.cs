using Microsoft.Extensions.Options;
using Nakama.FormationEnrollment.Domain;
using Nakama.Formations.Domain;
using Nakama.FormationsList.Domain;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.Shared.Infrastructure.Pagination;
using Nakama.Shared.Infrastructure.Storage;

namespace Nakama.FormationsList.Application.PageSearcher;

public static class FormationProgressFilter
{
    public const string InProgress = "in_progress";
    public const string Completed = "completed";
}

public static class FormationSummaryPageSearcher
{
    public sealed record Query(
        Guid UserId,
        bool IsAdmin,
        int Page,
        int? PageSize,
        string? Search,
        string? Status,
        bool Archived = false,
        string? Progress = null) : IQuery<PagedResponse<FormationSummaryResponse>>;

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
            IReadOnlyCollection<Guid>? formationIds = query.IsAdmin
                ? null
                : await this.FindLearnerFormationIds(query.Progress, enrollmentsByFormation, cancellationToken);
            string? status = query.IsAdmin ? query.Status : FormationStatus.Active.ToString();

            PagedResult<FormationSummary> result = await summaryRepository.SearchPage(
                new FormationSummaryPageSpecification(
                    formationIds,
                    query.Search,
                    status,
                    pageRequest.Skip,
                    pageRequest.PageSize,
                    query.IsAdmin && query.Archived),
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

        private async Task<IReadOnlyCollection<Guid>> FindLearnerFormationIds(
            string? progress,
            Dictionary<Guid, FormationEnrollment.Domain.FormationEnrollment> enrollmentsByFormation,
            CancellationToken cancellationToken)
        {
            List<Guid> enrolledIds = enrollmentsByFormation.Keys.ToList();
            if (progress is not (FormationProgressFilter.InProgress or FormationProgressFilter.Completed) || enrolledIds.Count == 0)
            {
                return enrolledIds;
            }

            IReadOnlyList<FormationSummary> summaries = await summaryRepository.Search(
                new FormationSummaryByIdsSpecification(enrolledIds), cancellationToken);
            HashSet<Guid> completedIds = summaries
                .Where(summary => summary.ModulesCount > 0
                                  && enrollmentsByFormation[summary.Id].Progress.CompletedModuleIds.Count >= summary.ModulesCount)
                .Select(summary => summary.Id)
                .ToHashSet();

            return progress == FormationProgressFilter.Completed
                ? completedIds.ToList()
                : enrolledIds.Where(id => !completedIds.Contains(id)).ToList();
        }
    }
}
