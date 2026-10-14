using System.Net.Http.Headers;

namespace Test.Api.Support;

public sealed record SessionCookie(string Value, string Raw)
{
    public const string Name = "nakama_refresh";

    public bool IsHttpOnly => this.Raw.Contains("httponly", StringComparison.OrdinalIgnoreCase);

    public bool IsSecure => this.Raw.Contains("secure", StringComparison.OrdinalIgnoreCase);

    public bool IsSameSiteStrict => this.Raw.Contains("samesite=strict", StringComparison.OrdinalIgnoreCase);

    public bool IsScopedToTheAuthRoutes => this.Raw.Contains("path=/api/auth", StringComparison.OrdinalIgnoreCase);

    public bool IsExpired => this.Value.Length == 0
        || this.Raw.Contains("expires=Thu, 01 Jan 1970", StringComparison.OrdinalIgnoreCase);

    public static SessionCookie? From(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? values))
        {
            return null;
        }

        string? raw = values.FirstOrDefault(value => value.StartsWith(Name + "=", StringComparison.Ordinal));

        return raw is null ? null : new SessionCookie(raw.Split(';')[0][(Name.Length + 1)..], raw);
    }

    public static SessionCookie RequiredFrom(HttpResponseMessage response) =>
        From(response) ?? throw new InvalidOperationException("The response did not set the refresh-token cookie.");
}
