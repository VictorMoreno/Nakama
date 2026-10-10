namespace Api.Configuration;

public static class LoggerExtensions
{
    public static IServiceCollection AddLogger(this IServiceCollection services)
    {
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddLog4Net("log4net.config");
        });

        return services;
    }
}
