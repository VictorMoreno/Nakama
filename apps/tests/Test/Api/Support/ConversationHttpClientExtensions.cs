namespace Test.Api.Support;

public sealed record AttachedFile(string FileName, byte[] Content);

public static class ConversationHttpClientExtensions
{
    public static Task<HttpResponseMessage> AskChatbot(
        this HttpClient client,
        Guid botKey,
        Guid conversationId,
        string title,
        string question,
        string accessToken,
        params AttachedFile[] files)
    {
        MultipartFormDataContent content = new()
        {
            { new StringContent(title), "title" },
            { new StringContent(question), "question" }
        };

        foreach (AttachedFile file in files)
        {
            content.Add(new ByteArrayContent(file.Content), "files", file.FileName);
        }

        return client.SendContent(HttpMethod.Post, $"/api/chatbots/{botKey}/conversations/{conversationId}/ask", accessToken, content);
    }

    public static Task<HttpResponseMessage> GetConversationHistory(this HttpClient client, Guid botKey, string accessToken) =>
        client.Send(HttpMethod.Get, $"/api/chatbots/{botKey}/conversation-history", accessToken);

    public static Task<HttpResponseMessage> GetAnalytics(this HttpClient client, string accessToken) =>
        client.Send(HttpMethod.Get, "/api/analytics", accessToken);
}
