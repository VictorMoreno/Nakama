namespace Api.Configuration;

public static class FileStorage
{
    public static string ResolvePath(IConfiguration configuration, IWebHostEnvironment env, string configKey)
    {
        var relative = configuration[$"Storage:{configKey}"]
            ?? throw new InvalidOperationException($"Missing configuration 'Storage:{configKey}'.");

        var path = Path.Combine(env.ContentRootPath, relative);
        Directory.CreateDirectory(path);
        return path;
    }
}
