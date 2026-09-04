using Asp.Versioning;

namespace Api.Configuration.Swagger;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
            {
                options.ApiVersionReader = new HeaderApiVersionReader("X-Api-Version");
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
            })
            .AddOpenApi();

        return services;
    }
}