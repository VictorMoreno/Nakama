using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Nakama.FormationDetail.Application;
using Nakama.FormationDetail.Application.Finder;
using Nakama.Formations.Application.DocumentFileFinder;
using Nakama.Formations.Domain.Errors;
using Nakama.LlmProviders.Domain;
using Nakama.Shared.Application;
using Nakama.Shared.Domain;

namespace Nakama.Formations.Application.QuizGenerator;

public static class FormationQuizGenerator
{
    public const int MaxQuestions = 4;

    public sealed record Query(
        Guid FormationId,
        IReadOnlyList<Guid> DocumentIds,
        int Provider,
        string Model,
        int QuestionCount,
        string Topic) : IQuery<GeneratedQuizResponse>;

    public sealed class Handler(Generator generator) : IQueryHandler<Query, GeneratedQuizResponse>
    {
        public async Task<GeneratedQuizResponse> Handle(Query query, CancellationToken ct) =>
            await generator.Execute(
                query.FormationId,
                query.DocumentIds,
                (LlmProvider)query.Provider,
                query.Model,
                query.QuestionCount,
                query.Topic,
                ct);
    }

    public sealed class Generator(IQueryBus queryBus, IServiceProvider serviceProvider) : IService
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        public async Task<GeneratedQuizResponse> Execute(
            Guid formationId,
            IReadOnlyList<Guid> documentIds,
            LlmProvider provider,
            string model,
            int questionCount,
            string topic,
            CancellationToken cancellationToken)
        {
            if (questionCount < 1 || questionCount > MaxQuestions)
            {
                throw FormationErrors.InvalidQuizQuestionCount(MaxQuestions);
            }

            if (string.IsNullOrWhiteSpace(model))
            {
                throw FormationErrors.QuizModelRequired;
            }

            ILlmClient llmClient = serviceProvider.GetKeyedService<ILlmClient>(provider.ToString())
                                   ?? throw FormationErrors.QuizProviderNotConnected(provider.ToString());

            FormationDetailResponse formation = await queryBus.Ask(new FormationDetailFinder.Query(formationId), cancellationToken);
            if (formation.Docs.Count == 0)
            {
                throw FormationErrors.QuizFormationWithoutDocuments;
            }

            List<Guid> selected = documentIds.Distinct().ToList();
            if (selected.Count == 0)
            {
                throw FormationErrors.QuizNoDocumentsSelected;
            }

            Guid? foreign = selected.Where(id => formation.Docs.All(document => document.Id != id)).Select(id => (Guid?)id).FirstOrDefault();
            if (foreign is not null)
            {
                throw FormationErrors.QuizDocumentNotInFormation(foreign.Value);
            }

            List<LlmDocument> documents = await this.LoadDocuments(formationId, selected, cancellationToken);

            StringBuilder answer = new();
            await foreach (string chunk in llmClient.AskStream(
                               model,
                               BuildSystemPrompt(questionCount, topic),
                               [new LlmMessage("user", "Genera las preguntas del examen.")],
                               documents,
                               cancellationToken))
            {
                answer.Append(chunk);
            }

            return Parse(answer.ToString(), questionCount);
        }

        private async Task<List<LlmDocument>> LoadDocuments(Guid formationId, List<Guid> documentIds, CancellationToken ct)
        {
            List<LlmDocument> documents = [];

            foreach (Guid documentId in documentIds)
            {
                FormationDocumentFileResponse file = await queryBus.Ask(new FormationDocumentFileFinder.Query(formationId, documentId), ct);
                LlmDocumentKind kind = file.ContentType == "application/pdf" ? LlmDocumentKind.Pdf : LlmDocumentKind.PlainText;
                documents.Add(new LlmDocument(file.FileName, kind, file.Content));
            }

            return documents;
        }

        private static string BuildSystemPrompt(int questionCount, string topic)
        {
            string subject = string.IsNullOrWhiteSpace(topic)
                ? "los documentos adjuntos"
                : $"el tema \"{topic.Trim()}\", basándote en los documentos adjuntos";

            return string.Join(
                "\n\n",
                "Eres un generador de preguntas de examen tipo test para una formación interna.",
                $"Genera exactamente {questionCount} preguntas sobre {subject}, sin usar información que no esté en ellos.",
                "Cada pregunta tiene exactamente 4 opciones y solo una es correcta; las opciones incorrectas deben ser plausibles.",
                "Responde únicamente con un array JSON, sin texto adicional ni bloques de código, con este formato: " +
                "[{\"question\":\"...\",\"options\":[\"...\",\"...\",\"...\",\"...\"],\"correctIndex\":0}]. " +
                "correctIndex es la posición de la opción correcta, empezando en 0.");
        }

        private static GeneratedQuizResponse Parse(string raw, int expectedQuestions)
        {
            List<DraftQuestion>? drafts;
            try
            {
                drafts = JsonSerializer.Deserialize<List<DraftQuestion>>(StripCodeFence(raw.Trim()), JsonOptions);
            }
            catch (JsonException)
            {
                throw FormationErrors.QuizGenerationFailed;
            }

            if (drafts is null || drafts.Count != expectedQuestions || !drafts.All(IsValid))
            {
                throw FormationErrors.QuizGenerationFailed;
            }

            return new GeneratedQuizResponse(drafts
                .Select(draft => new GeneratedQuestionResponse(
                    draft.Question!.Trim(),
                    draft.Options!.Select(option => option.Trim()).ToList(),
                    draft.CorrectIndex))
                .ToList());
        }

        private static string StripCodeFence(string text)
        {
            if (!text.StartsWith("```", StringComparison.Ordinal))
            {
                return text;
            }

            int firstLineEnd = text.IndexOf('\n');
            int lastFence = text.LastIndexOf("```", StringComparison.Ordinal);
            return firstLineEnd < 0 || lastFence <= firstLineEnd ? text : text[(firstLineEnd + 1)..lastFence].Trim();
        }

        private static bool IsValid(DraftQuestion draft) =>
            !string.IsNullOrWhiteSpace(draft.Question)
            && draft.Options is { Count: >= 2 }
            && draft.Options.All(option => !string.IsNullOrWhiteSpace(option))
            && draft.CorrectIndex >= 0
            && draft.CorrectIndex < draft.Options.Count;

        private sealed record DraftQuestion(string? Question, List<string>? Options, int CorrectIndex);
    }
}
