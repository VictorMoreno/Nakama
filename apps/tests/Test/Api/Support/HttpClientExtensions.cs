using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Test.Api.Support;

public static class HttpClientExtensions
{
    public static Task<HttpResponseMessage> LogIn(this HttpClient client, UserCredentials credentials) =>
        client.PostAsJsonAsync("/api/auth/login", new { credentials.Email, credentials.Password });

    public static Task<HttpResponseMessage> Refresh(this HttpClient client, string cookieValue) =>
        client.Send("/api/auth/refresh", HttpMethod.Post, cookieValue, null);

    public static Task<HttpResponseMessage> LogOut(this HttpClient client, string cookieValue) =>
        client.Send("/api/auth/logout", HttpMethod.Post, cookieValue, null);

    public static Task<HttpResponseMessage> Get(this HttpClient client, string path, string? accessToken) =>
        client.Send(path, HttpMethod.Get, null, accessToken);

    public static Task<HttpResponseMessage> SendJson<TBody>(
        this HttpClient client,
        HttpMethod method,
        string path,
        TBody body,
        string accessToken) =>
        client.Send(path, method, null, accessToken, JsonContent.Create(body));

    public static Task<HttpResponseMessage> SendContent(
        this HttpClient client,
        HttpMethod method,
        string path,
        string accessToken,
        HttpContent content) =>
        client.Send(path, method, null, accessToken, content);

    public static Task<HttpResponseMessage> Send(this HttpClient client, HttpMethod method, string path, string accessToken) =>
        client.Send(path, method, null, accessToken);

    private static Task<HttpResponseMessage> Send(
        this HttpClient client,
        string path,
        HttpMethod method,
        string? cookieValue,
        string? accessToken,
        HttpContent? content = null)
    {
        HttpRequestMessage request = new(method, path) { Content = content };

        if (cookieValue is not null)
        {
            request.Headers.Add("Cookie", $"{SessionCookie.Name}={cookieValue}");
        }

        if (accessToken is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return client.SendAsync(request);
    }
}
