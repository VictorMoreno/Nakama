namespace Nakama.FormationEnrollment.Domain;

public interface IFormationEnrollmentRepository
{
    Task<FormationEnrollment?> FindByFormationAndUser(Guid formationId, Guid userId, CancellationToken ct);

    Task<IReadOnlyList<FormationEnrollment>> FindAllForUser(Guid userId, CancellationToken ct);

    Task Save(FormationEnrollment enrollment, CancellationToken ct);

    Task Remove(FormationEnrollment enrollment, CancellationToken ct);
}
