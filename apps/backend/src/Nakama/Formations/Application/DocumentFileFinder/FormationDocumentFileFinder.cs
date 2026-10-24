using Nakama.Formations.Domain;
using Nakama.Formations.Domain.Errors;
using Nakama.Formations.Domain.Services;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;

namespace Nakama.Formations.Application.DocumentFileFinder;

public static class FormationDocumentFileFinder
{
    public sealed record Query(Guid FormationId, Guid DocumentId) : IQuery<FormationDocumentFileResponse>;

    public sealed class Handler(Finder finder) : IQueryHandler<Query, FormationDocumentFileResponse>
    {
        public async Task<FormationDocumentFileResponse> Handle(Query query, CancellationToken ct)
        {
            return await finder.Execute(FormationId.From(query.FormationId), query.DocumentId, ct);
        }
    }

    public sealed class Finder(IFormationsRepository repository, IFormationDocumentFileReader fileReader) : IService
    {
        private static readonly Dictionary<string, string> ContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = "application/pdf",
            [".md"] = "text/markdown",
            [".markdown"] = "text/markdown",
            [".txt"] = "text/plain"
        };

        private readonly FormationFinderById _finder = new(repository);

        public async Task<FormationDocumentFileResponse> Execute(FormationId formationId, Guid documentId, CancellationToken cancellationToken)
        {
            Formation formation = await this._finder.Execute(formationId, cancellationToken);

            FormationDocument document = formation.Documents.FirstOrDefault(d => d.Id == documentId)
                                         ?? throw FormationErrors.DocumentNotFound(documentId);

            byte[] content = await fileReader.Read(document.FileName, cancellationToken)
                             ?? throw FormationErrors.DocumentNotFound(documentId);

            string extension = Path.GetExtension(document.FileName);
            string contentType = ContentTypes.GetValueOrDefault(extension, "application/octet-stream");
            string downloadName = document.Name.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
                ? document.Name
                : document.Name + extension;

            return new FormationDocumentFileResponse(content, contentType, downloadName);
        }
    }
}
