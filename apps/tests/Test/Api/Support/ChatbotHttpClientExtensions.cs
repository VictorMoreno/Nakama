using Nakama.Chatbots.Application.Creator;
using Nakama.Chatbots.Domain;

namespace Test.Api.Support;

public static class ChatbotHttpClientExtensions
{
    public static Task<HttpResponseMessage> CreateChatbot(
        this HttpClient client,
        ChatbotCreator.Command command,
        string accessToken) =>
        client.SendJson(
            HttpMethod.Put,
            $"/api/chatbots/{command.Id}",
            new
            {
                command.Label,
                command.Description,
                command.Status,
                command.Socratic,
                command.ExplicaMas,
                command.Provider,
                command.Model
            },
            accessToken);

    public static Task<HttpResponseMessage> UpdateChatbotStatus(
        this HttpClient client,
        Guid id,
        int status,
        string accessToken) =>
        client.SendJson(HttpMethod.Patch, $"/api/chatbots/{id}/status", new { Status = status }, accessToken);

    public static Task<HttpResponseMessage> ArchiveChatbot(this HttpClient client, Guid id, string accessToken) =>
        client.Send(HttpMethod.Post, $"/api/chatbots/{id}/archive", accessToken);

    public static Task<HttpResponseMessage> UnarchiveChatbot(this HttpClient client, Guid id, string accessToken) =>
        client.Send(HttpMethod.Post, $"/api/chatbots/{id}/unarchive", accessToken);

    public static Task<HttpResponseMessage> AddChatbotTeamMember(
        this HttpClient client,
        Guid id,
        Guid userId,
        TeamRole role,
        string accessToken) =>
        client.SendJson(HttpMethod.Post, $"/api/chatbots/{id}/team", new { UserId = userId, Role = (int)role }, accessToken);

    public static Task<HttpResponseMessage> RemoveChatbotTeamMember(
        this HttpClient client,
        Guid id,
        Guid userId,
        string accessToken) =>
        client.Send(HttpMethod.Delete, $"/api/chatbots/{id}/team/{userId}", accessToken);

    public static Task<HttpResponseMessage> ListChatbots(this HttpClient client, string accessToken) =>
        client.Send(HttpMethod.Get, "/api/chatbots", accessToken);

    public static Task<HttpResponseMessage> ListChatbotsPage(
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

        return client.Send(HttpMethod.Get, $"/api/chatbots/paged{query}", accessToken);
    }

    public static Task<HttpResponseMessage> GetChatbot(this HttpClient client, Guid id, string accessToken) =>
        client.Send(HttpMethod.Get, $"/api/chatbots/{id}", accessToken);
}
