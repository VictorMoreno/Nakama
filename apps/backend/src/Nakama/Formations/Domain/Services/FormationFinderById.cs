using Nakama.Formations.Domain.Errors;

namespace Nakama.Formations.Domain.Services;

public sealed class FormationFinderById(IFormationsRepository repository)
{
    public async Task<Formation> Execute(FormationId id, CancellationToken cancellationToken)
    {
        return await repository.FindById(id, cancellationToken)
            ?? throw FormationErrors.NotFound(id);
    }
}
