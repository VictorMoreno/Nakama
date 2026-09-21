namespace Api.Configuration;

public static class IconStorage
{
    private const long MaxBytes = 2 * 1024 * 1024;

    private static readonly Dictionary<string, string> ExtensionsByContentType = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/png"] = ".png",
        ["image/jpeg"] = ".jpg",
        ["image/jpg"] = ".jpg",
    };

    public static string ResolvePath(IConfiguration configuration, IWebHostEnvironment env, string configKey) =>
        FileStorage.ResolvePath(configuration, env, configKey);

    public static async Task<string> SaveAsync(string basePath, Guid id, IFormFile file, CancellationToken ct)
    {
        if (!ExtensionsByContentType.TryGetValue(file.ContentType, out var extension))
        {
            throw new BadHttpRequestException("Unsupported image type. Only PNG and JPEG are allowed.");
        }

        // An icon replaces whatever was there before (1 icon per chatbot/formation) - unlike
        // documents, which are always distinct new entries, so no delete-before-save there.
        DeleteExisting(basePath, id);
        return await FileStorage.SaveAsync(basePath, id.ToString(), extension, file, MaxBytes, ct);
    }

    public static void DeleteExisting(string basePath, Guid id) => FileStorage.DeleteExisting(basePath, id.ToString());
}
