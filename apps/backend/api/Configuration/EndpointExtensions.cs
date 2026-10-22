namespace Api.Configuration;

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblyOf<Program>()
            .AddClasses(classes => classes.AssignableTo<IEndpoint>())
            .AsImplementedInterfaces()
            .WithSingletonLifetime());

        return services;
    }

    public static WebApplication MapEndpoints(this WebApplication app)
    {
        RouteGroupBuilder apiGroup = app.MapGroup("api").AddEndpointFilter<UnitOfWorkEndpointFilter>();

        using (IServiceScope scope = app.Services.CreateScope())
        {
            IEnumerable<IEndpoint> modules = scope.ServiceProvider.GetServices<IEndpoint>();
            foreach (IEndpoint module in modules)
            {
                module.MapEndpoints(apiGroup);
            }
        }

        return app;
    }
}