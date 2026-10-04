using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Configuration.Swagger;

// One SwaggerDoc per API version - SwaggerGenOptions.DocInclusionPredicate defaults to
// matching ApiDescription.GroupName against the document name, which AddApiExplorer already
// populates via GroupNameFormat, so no custom predicate is needed here.
public sealed class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    : IConfigureNamedOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = "Nakama API",
                Version = description.ApiVersion.ToString(),
                Description = description.IsDeprecated ? "This API version has been deprecated." : null
            });
        }
    }

    public void Configure(string? name, SwaggerGenOptions options) => this.Configure(options);
}
