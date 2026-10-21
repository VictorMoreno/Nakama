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

    public static Task<HttpResponseMessage> ArchiveFormation(this HttpClient client, Guid id, string accessToken) =>
        client.Send(HttpMethod.Post, $"/api/formations/{id}/archive", accessToken);

    public static Task<HttpResponseMessage> ListFormationsPage(
        this HttpClient client,
        string accessToken,
        int? page = null,
        int? pageSize = null,
        string? search = null,
        string? status = null)
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

        string query = parameters.Count == 0 ? string.Empty : $"?{string.Join("&", parameters)}";

        return client.Send(HttpMethod.Get, $"/api/formations/paged{query}", accessToken);
    }
}
