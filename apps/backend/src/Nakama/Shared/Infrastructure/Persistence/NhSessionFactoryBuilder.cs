using Microsoft.Extensions.Configuration;
using Nakama.Shared.Infrastructure.Persistence.Nh;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;

namespace Nakama.Shared.Infrastructure.Persistence;

internal static class NhSessionFactoryBuilder
{
    public static NHibernate.ISessionFactory Build(IConfiguration configuration, string connectionStringName)
    {
        var connectionString = configuration.GetConnectionString(connectionStringName)
            ?? throw new InvalidOperationException(
                $"Missing connection string 'ConnectionStrings:{connectionStringName}'.");

        var cfg = new Configuration();
        cfg.DataBaseIntegration(db =>
        {
            db.Dialect<PostgreSQLDialect>();
            db.Driver<NpgsqlDriver>();
            db.ConnectionString = connectionString;
        });

        NhMappings.AddMappings(cfg);

        return cfg.BuildSessionFactory();
    }
}
