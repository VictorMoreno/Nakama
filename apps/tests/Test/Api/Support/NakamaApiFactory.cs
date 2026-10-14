using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Test.Modules.Authentication.Domain;
using Test.Modules.Shared.Infrastructure;

namespace Test.Api.Support;

public sealed class NakamaApiFactory(TestDatabase database) : WebApplicationFactory<Program>
{
    private readonly string _storageRoot = Path.Combine(Path.GetTempPath(), $"nakama-e2e-{Guid.NewGuid():N}");

    private readonly string _signingKey = JwtOptionsMother.Random().SigningKey;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Nakama", database.ConnectionString);
        builder.UseSetting("Jwt:SigningKey", this._signingKey);
        builder.UseSetting("Storage:ChatbotIconsPath", Path.Combine(this._storageRoot, "chatbot-icons"));
        builder.UseSetting("Storage:FormationIconsPath", Path.Combine(this._storageRoot, "formation-icons"));
        builder.UseSetting("Storage:ChatbotDocumentsPath", Path.Combine(this._storageRoot, "chatbot-documents"));
        builder.UseSetting("Storage:ConversationAttachmentsPath", Path.Combine(this._storageRoot, "conversation-attachments"));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && Directory.Exists(this._storageRoot))
        {
            Directory.Delete(this._storageRoot, true);
        }
    }
}
