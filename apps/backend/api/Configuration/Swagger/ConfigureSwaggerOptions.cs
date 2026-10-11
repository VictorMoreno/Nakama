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
    private const string BearerSchemeName = "Bearer";

    public void Configure(SwaggerGenOptions options)
    {
        foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = "Nakama API",
                Version = description.ApiVersion.ToString(),
                Description = description.IsDeprecated ? "This API version has been deprecated." : null
            });
        }

        options.AddSecurityDefinition(BearerSchemeName, new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "JWT access token returned by POST /api/auth/login."
        });

        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(BearerSchemeName, document)] = []
        });
    }

    public void Configure(string? name, SwaggerGenOptions options) => this.Configure(options);
}
