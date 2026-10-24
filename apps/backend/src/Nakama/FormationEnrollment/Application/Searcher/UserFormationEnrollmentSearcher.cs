using Nakama.FormationEnrollment.Domain;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;

namespace Nakama.FormationEnrollment.Application.Searcher;

public static class UserFormationEnrollmentSearcher
{
    public sealed record Query(Guid UserId) : IQuery<IReadOnlyCollection<FormationEnrollmentResponse>>;

    public sealed class Handler(Searcher searcher) : IQueryHandler<Query, IReadOnlyCollection<FormationEnrollmentResponse>>
    {
        public async Task<IReadOnlyCollection<FormationEnrollmentResponse>> Handle(Query query, CancellationToken ct)
        {
            IReadOnlyList<Domain.FormationEnrollment> enrollments = await searcher.Execute(query.UserId, ct);
            return enrollments.Select(enrollment => (FormationEnrollmentResponse)enrollment).ToList();
        }
    }

    public sealed class Searcher(IFormationEnrollmentRepository repository) : IService
    {
        public Task<IReadOnlyList<Domain.FormationEnrollment>> Execute(Guid userId, CancellationToken cancellationToken) =>
            repository.Search(new FormationEnrollmentByUserSpecification(userId), cancellationToken);
    }
}
