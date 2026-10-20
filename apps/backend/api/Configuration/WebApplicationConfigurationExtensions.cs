using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Nakama.Shared.Infrastructure.Persistence;
using Nakama.Shared.Infrastructure.Storage;

namespace Api.Configuration;

public static class WebApplicationConfigurationExtensions
{
    public static void Configure(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger(options =>
            {
                options.RouteTemplate = "openapi/{documentName}.json";
            });

            app.UseSwaggerUI(options =>
            {
                options.RoutePrefix = "api/docs";

                if (app.DescribeApiVersions().Reverse().Any())
                {
                    foreach (string groupName in app.DescribeApiVersions().Reverse().Select(description => description.GroupName))
                    {
                        options.SwaggerEndpoint($"/openapi/{groupName}.json",
                            groupName.ToUpperInvariant());
                    }
                }
            });
        }

        MigrationRunner.Run(app.Configuration.GetConnectionString("Nakama")!);

        app.UseExceptionHandler();
        app.UseStatusCodePages();

        StorageOptions storage = app.Services.GetRequiredService<IOptions<StorageOptions>>().Value;

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(FileStorage.ResolvePath(storage.ChatbotIconsPath, app.Environment)),
            RequestPath = storage.ChatbotIconsRoute
        });
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(FileStorage.ResolvePath(storage.FormationIconsPath, app.Environment)),
            RequestPath = storage.FormationIconsRoute
        });

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapEndpoints();
        app.MapHealthChecks("/health").AllowAnonymous();
        app.MapFallbackToFile("{*path:nonfile:regex(^(?!api(/|$)).*$)}", "index.html").AllowAnonymous();
    }
}
