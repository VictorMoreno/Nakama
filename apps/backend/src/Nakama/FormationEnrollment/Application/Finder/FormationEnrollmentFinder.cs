using Nakama.FormationEnrollment.Domain;
using Nakama.FormationEnrollment.Domain.Errors;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;

namespace Nakama.FormationEnrollment.Application.Finder;

public static class FormationEnrollmentFinder
{
    public sealed record Query(Guid FormationId, Guid UserId) : IQuery<FormationEnrollmentResponse>;

    public sealed class Handler(Finder finder) : IQueryHandler<Query, FormationEnrollmentResponse>
    {
        public async Task<FormationEnrollmentResponse> Handle(Query query, CancellationToken ct)
        {
            var enrollment = await finder.Execute(query.FormationId, query.UserId, ct);
            return (FormationEnrollmentResponse)enrollment;
        }
    }

    public sealed class Finder(IFormationEnrollmentRepository repository) : IService
    {
        public async Task<Domain.FormationEnrollment> Execute(Guid formationId, Guid userId, CancellationToken cancellationToken) =>
            await repository.FindByFormationAndUser(formationId, userId, cancellationToken)
                ?? throw FormationEnrollmentErrors.NotEnrolled(formationId, userId);
    }
}
