using Nakama.Chatbots.Domain;
using Nakama.Chatbots.Domain.Errors;
using Nakama.Chatbots.Domain.Services;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;

namespace Nakama.Chatbots.Application.DocumentFileFinder;

public static class ChatbotDocumentFileFinder
{
    public sealed record Query(Guid ChatbotId, Guid DocumentId) : IQuery<ChatbotDocumentFileResponse>;

    public sealed class Handler(Finder finder) : IQueryHandler<Query, ChatbotDocumentFileResponse>
    {
        public async Task<ChatbotDocumentFileResponse> Handle(Query query, CancellationToken ct)
        {
            return await finder.Execute(ChatbotId.From(query.ChatbotId), query.DocumentId, ct);
        }
    }

    public sealed class Finder(IChatbotsRepository repository, IChatbotDocumentFileReader fileReader) : IService
    {
        private static readonly Dictionary<string, string> ContentTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            [".pdf"] = "application/pdf",
            [".md"] = "text/markdown",
            [".markdown"] = "text/markdown",
            [".txt"] = "text/plain"
        };

        private readonly ChatbotFinderById _finder = new(repository);

        public async Task<ChatbotDocumentFileResponse> Execute(ChatbotId chatbotId, Guid documentId, CancellationToken cancellationToken)
        {
            Chatbot chatbot = await this._finder.Execute(chatbotId, cancellationToken);

            ChatbotDocument document = chatbot.Documents.FirstOrDefault(d => d.Id == documentId)
                                       ?? throw ChatbotErrors.DocumentNotFound(documentId);

            byte[] content = await fileReader.Read(document.FileName, cancellationToken)
                             ?? throw ChatbotErrors.DocumentNotFound(documentId);

            string extension = Path.GetExtension(document.FileName);
            string contentType = ContentTypes.GetValueOrDefault(extension, "application/octet-stream");
            string downloadName = document.Name.EndsWith(extension, StringComparison.OrdinalIgnoreCase)
                ? document.Name
                : document.Name + extension;

            return new ChatbotDocumentFileResponse(content, contentType, downloadName);
        }
    }
}
