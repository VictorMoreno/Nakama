using System.Reflection;
using DbUp;

namespace Nakama.Shared.Infrastructure.Persistence;

/// <summary>
/// Runs the versioned .sql scripts embedded from Shared/Infrastructure/Persistence/Migrations/
/// against Postgres via DbUp (own SchemaVersions journal table tracks what already ran).
/// The scripts themselves are generated from the real NHibernate mapping metadata via
/// SchemaExport, not hand-written - see ADR entry for the full rationale.
/// </summary>
public static class MigrationRunner
{
    public static void Run(string connectionString)
    {
        var upgrader = DeployChanges.To
            .PostgresqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            throw new InvalidOperationException("Database migration failed.", result.Error);
        }
    }
}
