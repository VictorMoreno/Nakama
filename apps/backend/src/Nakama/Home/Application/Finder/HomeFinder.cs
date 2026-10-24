using Nakama.FormationDetail.Application;
using Nakama.FormationDetail.Application.Finder;
using Nakama.FormationEnrollment.Application;
using Nakama.FormationEnrollment.Application.Searcher;
using Nakama.Formations.Domain;
using Nakama.QuestionActivity.Application;
using Nakama.QuestionActivity.Application.Finder;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;
using Nakama.UserActivity.Application;
using Nakama.UserActivity.Application.Finder;

namespace Nakama.Home.Application.Finder;

public static class HomeFinder
{
    public sealed record Query(Guid UserId) : IQuery<HomeResponse>;

    public sealed class Handler(Finder finder) : IQueryHandler<Query, HomeResponse>
    {
        public async Task<HomeResponse> Handle(Query query, CancellationToken ct) => await finder.Execute(query.UserId, ct);
    }

    public sealed class Finder(IQueryBus queryBus) : IService
    {
        private const int CurrentWeekOnly = 1;

        private const int ActiveDaysWindow = 30;

        public async Task<HomeResponse> Execute(Guid userId, CancellationToken cancellationToken)
        {
            QuestionActivityResponse questions = await queryBus.Ask(new QuestionActivityFinder.Query(CurrentWeekOnly, userId), cancellationToken);
            UserActivityResponse activity = await queryBus.Ask(new UserActivityFinder.Query(ActiveDaysWindow, userId), cancellationToken);
            IReadOnlyCollection<FormationEnrollmentResponse> enrollments =
                await queryBus.Ask(new UserFormationEnrollmentSearcher.Query(userId), cancellationToken);

            List<HomeFormationRow> formations = [];
            List<HomePendingExamRow> pendingExams = [];
            foreach (FormationEnrollmentResponse enrollment in enrollments)
            {
                FormationDetailResponse detail = await queryBus.Ask(new FormationDetailFinder.Query(enrollment.FormationId), cancellationToken);
                if (detail.Archived || detail.Status != FormationStatus.Active.ToString())
                {
                    continue;
                }

                formations.Add(ToFormationRow(detail, enrollment));
                pendingExams.AddRange(ToPendingExams(detail, enrollment));
            }

            return new HomeResponse(
                questions.QuestionsThisWeek,
                activity.Days.Count(day => day.ActiveUsers > 0),
                formations,
                pendingExams);
        }

        private static HomeFormationRow ToFormationRow(FormationDetailResponse detail, FormationEnrollmentResponse enrollment)
        {
            int completedModules = detail.Modules.Count(module => enrollment.CompletedModuleIds.Contains(module.Id));
            int progressPct = detail.Modules.Count == 0 ? 0 : completedModules * 100 / detail.Modules.Count;
            bool completed = detail.Modules.Count > 0 && completedModules == detail.Modules.Count;
            string? nextModuleTitle = detail.Modules.FirstOrDefault(module => !enrollment.CompletedModuleIds.Contains(module.Id))?.Title;

            return new HomeFormationRow(detail.Id, detail.Title, progressPct, completedModules, detail.Modules.Count, nextModuleTitle, completed);
        }

        private static IEnumerable<HomePendingExamRow> ToPendingExams(FormationDetailResponse detail, FormationEnrollmentResponse enrollment) =>
            detail.Modules
                .Where(module => module.Type == ModuleType.Exam.ToString() && !enrollment.CompletedModuleIds.Contains(module.Id))
                .Select(module => new
                {
                    Module = module,
                    LastAttempt = enrollment.ExamAttempts.LastOrDefault(attempt => attempt.ModuleId == module.Id)
                })
                .Where(item => item.LastAttempt is not null)
                .Select(item => new HomePendingExamRow(
                    detail.Id,
                    detail.Title,
                    item.Module.Id,
                    item.Module.Title,
                    item.LastAttempt!.Score,
                    item.Module.PassingScore ?? 0));
    }
}
