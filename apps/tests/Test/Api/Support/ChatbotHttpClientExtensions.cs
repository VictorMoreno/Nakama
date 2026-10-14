using Nakama.Chatbots.Application.Creator;

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

    public static Task<HttpResponseMessage> ListChatbots(this HttpClient client, string accessToken) =>
        client.Send(HttpMethod.Get, "/api/chatbots", accessToken);

    public static Task<HttpResponseMessage> GetChatbot(this HttpClient client, Guid id, string accessToken) =>
        client.Send(HttpMethod.Get, $"/api/chatbots/{id}", accessToken);
}
