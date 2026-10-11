namespace Api.Configuration;

public static class RefreshTokenCookie
{
    private const string Name = "nakama_refresh";
    private const string Path = "/api/auth";

    public static void Set(HttpResponse response, string value, DateTime expiresAt) =>
        response.Cookies.Append(Name, value, Options(expiresAt));

    public static void Clear(HttpResponse response) =>
        response.Cookies.Delete(Name, Options(null));

    public static string? Read(HttpRequest request) =>
        request.Cookies.TryGetValue(Name, out string? value) && !string.IsNullOrWhiteSpace(value) ? value : null;

    private static CookieOptions Options(DateTime? expiresAt) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = Path,
        Expires = expiresAt is null ? null : new DateTimeOffset(DateTime.SpecifyKind(expiresAt.Value, DateTimeKind.Utc))
    };
}
