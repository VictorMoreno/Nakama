namespace Api.Configuration;

public static class IconStorage
{
    public static string ResolvePath(IConfiguration configuration, IWebHostEnvironment env, string configKey) =>
        FileStorage.ResolvePath(configuration, env, configKey);
}
