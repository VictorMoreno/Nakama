using Microsoft.Extensions.FileProviders;
using Nakama.Shared.Infrastructure.Persistence;

namespace Api.Configuration;

public static class WebApplicationConfigurationExtensions
{
    public static void Configure(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi()
                .WithDocumentPerVersion();

            MigrationRunner.Run(app.Configuration.GetConnectionString("Nakama")!);
        }

        app.UseExceptionHandler();

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(IconStorage.ResolvePath(app.Configuration, app.Environment, "ChatbotIconsPath")),
            RequestPath = "/api/chatbots/icons"
        });
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(IconStorage.ResolvePath(app.Configuration, app.Environment, "FormationIconsPath")),
            RequestPath = "/api/formations/icons"
        });
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(DocumentStorage.ResolvePath(app.Configuration, app.Environment, "ChatbotDocumentsPath")),
            RequestPath = "/api/chatbots/documents"
        });

        app.MapEndpoints();
        app.MapHealthChecks("/health");

        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = "api/docs";

            if (app.DescribeApiVersions().Reverse().Any())
            {
                foreach (var description in app.DescribeApiVersions().Reverse())
                {
                    options.SwaggerEndpoint($"/openapi/{description.GroupName}.json",
                        description.GroupName.ToUpperInvariant());
                }
            }
        });
    }
}