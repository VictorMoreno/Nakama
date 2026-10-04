namespace Api.Configuration;

public static class DocumentStorage
{
    public static string ResolvePath(IConfiguration configuration, IWebHostEnvironment env, string configKey) =>
        FileStorage.ResolvePath(configuration, env, configKey);
}
