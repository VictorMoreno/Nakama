namespace Api.Configuration;

/// <summary>
/// Shared primitives behind <see cref="IconStorage"/> and <see cref="DocumentStorage"/>: resolve
/// a configured disk path, write a file under a size limit, delete by exact name or by glob.
/// Content-type/extension validation is caller-specific (images vs. documents validate
/// differently), so it stays out of this shared layer.
/// </summary>
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

    public static async Task<string> SaveAsync(string basePath, string id, string extension, Stream content, long length, long maxBytes, CancellationToken ct)
    {
        if (length > maxBytes)
        {
            throw new BadHttpRequestException($"File exceeds the {maxBytes / (1024 * 1024)} MB limit.");
        }

        var fileName = $"{id}{extension}";
        await using (var stream = File.Create(Path.Combine(basePath, fileName)))
        {
            await content.CopyToAsync(stream, ct);
        }

        return fileName;
    }

    public static void DeleteExisting(string basePath, string id)
    {
        foreach (var existing in Directory.EnumerateFiles(basePath, $"{id}.*"))
        {
            File.Delete(existing);
        }
    }

    public static void Delete(string basePath, string fileName)
    {
        var path = Path.Combine(basePath, fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
