using Microsoft.Extensions.Configuration;
using Nakama.Shared.Infrastructure.Persistence.Nh;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Driver;

namespace Nakama.Shared.Infrastructure.Persistence;

/// <summary>
/// Shared build logic for the write/read <see cref="NHibernate.ISessionFactory"/> singletons.
/// Database-per-tenant resolution is explicitly out of scope for this pass (see the plan) -
/// this reads a single connection string from <c>ConnectionStrings:&lt;name&gt;</c> as a
/// placeholder, leaving the wiring ready to accept a per-tenant <see cref="Configuration"/>
/// later.
/// </summary>
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
