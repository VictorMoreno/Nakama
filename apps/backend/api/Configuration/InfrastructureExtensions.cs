using Api.Configuration.Swagger;

namespace Api.Configuration;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSwagger()
            .AddEndpoints();

        services.AddHealthChecks();
        
        return services;
    }
}