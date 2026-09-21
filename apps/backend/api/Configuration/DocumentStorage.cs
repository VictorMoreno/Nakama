namespace Api.Configuration;

public static class DocumentStorage
{
    private const long MaxBytes = 20 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".md", ".markdown", ".txt",
    };

    public static string ResolvePath(IConfiguration configuration, IWebHostEnvironment env, string configKey) =>
        FileStorage.ResolvePath(configuration, env, configKey);

    public static async Task<string> SaveAsync(string basePath, Guid documentId, IFormFile file, CancellationToken ct)
    {
        // Content-type sniffing for Markdown/plain-text is unreliable across browsers/OSes
        // (often reported as application/octet-stream) - validate by the original filename's
        // extension instead, unlike icons where image/* content-types are reliably reported.
        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            throw new BadHttpRequestException("Unsupported document type. Only PDF, Markdown, and plain text files are allowed.");
        }

        // Each upload is a distinct new document (unlike icons, which replace a single slot),
        // so there's no existing file to clean up before saving this one.
        return await FileStorage.SaveAsync(basePath, documentId.ToString(), extension, file, MaxBytes, ct);
    }

    public static void Delete(string basePath, string fileName) => FileStorage.Delete(basePath, fileName);
}
