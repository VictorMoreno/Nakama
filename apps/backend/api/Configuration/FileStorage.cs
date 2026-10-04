namespace Api.Configuration;

public static class FileStorage
{
    public static string ResolvePath(string relativePath, IWebHostEnvironment env)
    {
        var path = Path.Combine(env.ContentRootPath, relativePath);
        Directory.CreateDirectory(path);
        return path;
    }
}
