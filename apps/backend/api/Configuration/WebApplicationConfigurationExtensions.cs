namespace Api.Configuration;

public static class WebApplicationConfigurationExtensions
{
    public static void Configure(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi()
                .WithDocumentPerVersion();
        }

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