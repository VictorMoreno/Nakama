using Nakama.Shared.Infrastructure.Persistence;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Test.Modules.Shared.Infrastructure;

public static class PostgresContainer
{
    private const string TemplateDatabase = "nakama_template";

    private static readonly Lazy<Task<PostgreSqlContainer>> Container = new(StartWithMigratedTemplate);

    public static async Task<TestDatabase> CreateDatabase()
    {
        PostgreSqlContainer container = await Container.Value;
        string name = $"test_{Guid.NewGuid():N}";

        await Execute(container, $"CREATE DATABASE \"{name}\" TEMPLATE {TemplateDatabase}");

        return new TestDatabase(name, ConnectionStringOf(container, name));
    }

    public static async Task DropDatabase(TestDatabase database)
    {
        PostgreSqlContainer container = await Container.Value;

        await Execute(container, $"DROP DATABASE IF EXISTS \"{database.Name}\" WITH (FORCE)");
    }

    public static async Task Stop()
    {
        if (!Container.IsValueCreated)
        {
            return;
        }

        PostgreSqlContainer container = await Container.Value;
        await container.DisposeAsync();
    }

    private static async Task<PostgreSqlContainer> StartWithMigratedTemplate()
    {
        PostgreSqlContainer container = new PostgreSqlBuilder("postgres:18-alpine").Build();
        await container.StartAsync();

        await Execute(container, $"CREATE DATABASE {TemplateDatabase}");
        MigrationRunner.Run(ConnectionStringOf(container, TemplateDatabase));
        NpgsqlConnection.ClearAllPools();

        return container;
    }

    private static string ConnectionStringOf(PostgreSqlContainer container, string database)
    {
        NpgsqlConnectionStringBuilder builder = new(container.GetConnectionString()) { Database = database, Pooling = false };

        return builder.ConnectionString;
    }

    private static async Task Execute(PostgreSqlContainer container, string sql)
    {
        await using NpgsqlConnection connection = new(container.GetConnectionString());
        await connection.OpenAsync();
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync();
    }
}

public sealed record TestDatabase(string Name, string ConnectionString);
