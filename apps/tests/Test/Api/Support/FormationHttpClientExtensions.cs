using Nakama.Formations.Domain;

namespace Test.Api.Support;

public static class FormationHttpClientExtensions
{
    public static Task<HttpResponseMessage> CreateFormation(
        this HttpClient client,
        Guid id,
        string title,
        string audience,
        string description,
        FormationStatus status,
        string accessToken) =>
        client.SendJson(
            HttpMethod.Put,
            $"/api/formations/{id}",
            new { Title = title, Audience = audience, Description = description, Status = (int)status },
            accessToken);

    public static Task<HttpResponseMessage> AddFormationDocument(
        this HttpClient client,
        Guid formationId,
        Guid documentId,
        AttachedFile file,
        string accessToken)
    {
        MultipartFormDataContent content = new()
        {
            { new StringContent(file.FileName), "name" },
            { new StringContent("Subida manual"), "origin" },
            { new ByteArrayContent(file.Content), "file", file.FileName }
        };

        return client.SendContent(HttpMethod.Put, $"/api/formations/{formationId}/documents/{documentId}", accessToken, content);
    }

    public static Task<HttpResponseMessage> RemoveFormationDocument(this HttpClient client, Guid formationId, Guid documentId, string accessToken) =>
        client.Send(HttpMethod.Delete, $"/api/formations/{formationId}/documents/{documentId}", accessToken);

    public static Task<HttpResponseMessage> DownloadFormationDocument(this HttpClient client, Guid formationId, Guid documentId, string? accessToken) =>
        client.Get($"/api/formations/{formationId}/documents/{documentId}/file", accessToken);

    public static Task<HttpResponseMessage> GetFormation(this HttpClient client, Guid id, string accessToken) =>
        client.Get($"/api/formations/{id}", accessToken);

    public static Task<HttpResponseMessage> GenerateQuizQuestions(
        this HttpClient client,
        Guid formationId,
        IReadOnlyList<Guid> documentIds,
        int provider,
        string model,
        int questionCount,
        string topic,
        string accessToken) =>
        client.SendJson(
            HttpMethod.Post,
            $"/api/formations/{formationId}/quiz-questions",
            new { DocumentIds = documentIds, Provider = provider, Model = model, QuestionCount = questionCount, Topic = topic },
            accessToken);

    public static Task<HttpResponseMessage> AddFormationExamModule(
        this HttpClient client,
        Guid formationId,
        Guid moduleId,
        int passingScore,
        IReadOnlyList<int> correctIndexes,
        string accessToken) =>
        client.SendJson(
            HttpMethod.Put,
            $"/api/formations/{formationId}/modules/{moduleId}",
            new
            {
                Title = "Examen",
                Type = (int)ModuleType.Exam,
                Summary = "Comprueba lo aprendido",
                Gated = false,
                AiDraft = false,
                Quiz = correctIndexes.Select(correctIndex => new { Question = "¿Cuál?", Options = new[] { "A", "B", "C" }, CorrectIndex = correctIndex }).ToList(),
                PassingScore = passingScore
            },
            accessToken);

    public static Task<HttpResponseMessage> AddFormationTeamMember(this HttpClient client, Guid formationId, Guid userId, string accessToken) =>
        client.SendJson(HttpMethod.Post, $"/api/formations/{formationId}/team", new { UserId = userId }, accessToken);

    public static Task<HttpResponseMessage> GetFormationEnrollment(this HttpClient client, Guid formationId, string accessToken) =>
        client.Get($"/api/formations/{formationId}/enrollment", accessToken);

    public static Task<HttpResponseMessage> CompleteFormationModule(this HttpClient client, Guid formationId, Guid moduleId, string accessToken) =>
        client.Send(HttpMethod.Post, $"/api/formations/{formationId}/modules/{moduleId}/complete", accessToken);

    public static Task<HttpResponseMessage> SubmitFormationExam(
        this HttpClient client,
        Guid formationId,
        Guid moduleId,
        IReadOnlyList<int> answers,
        string accessToken) =>
        client.SendJson(HttpMethod.Post, $"/api/formations/{formationId}/modules/{moduleId}/exam", new { Answers = answers }, accessToken);

    public static Task<HttpResponseMessage> ArchiveFormation(this HttpClient client, Guid id, string accessToken) =>
        client.Send(HttpMethod.Post, $"/api/formations/{id}/archive", accessToken);

    public static Task<HttpResponseMessage> UnarchiveFormation(this HttpClient client, Guid id, string accessToken) =>
        client.Send(HttpMethod.Post, $"/api/formations/{id}/unarchive", accessToken);

    public static Task<HttpResponseMessage> ListFormationsPage(
        this HttpClient client,
        string accessToken,
        int? page = null,
        int? pageSize = null,
        string? search = null,
        string? status = null,
        bool? archived = null)
    {
        List<string> parameters = [];

        if (page is not null)
        {
            parameters.Add($"page={page}");
        }

        if (pageSize is not null)
        {
            parameters.Add($"pageSize={pageSize}");
        }

        if (search is not null)
        {
            parameters.Add($"search={Uri.EscapeDataString(search)}");
        }

        if (status is not null)
        {
            parameters.Add($"status={Uri.EscapeDataString(status)}");
        }

        if (archived is not null)
        {
            parameters.Add($"archived={archived.Value.ToString().ToLowerInvariant()}");
        }

        string query = parameters.Count == 0 ? string.Empty : $"?{string.Join("&", parameters)}";

        return client.Send(HttpMethod.Get, $"/api/formations/paged{query}", accessToken);
    }
}
