using Microsoft.Extensions.Configuration;
using Nakama.Shared.Infrastructure.Persistence;

namespace Test.Modules.Shared.Infrastructure;

[Category("Infrastructure")]
public abstract class InfrastructureTestCase
{
    private TestDatabase _database = null!;

    private NhWriteSessionFactory _writeSessions = null!;

    protected IWriteSessionFactory WriteSessions => this._writeSessions;

    protected async Task Execute(string sql) => await PostgresContainer.Run(this._database, sql);

    [SetUp]
    public async Task InfrastructureSetUp()
    {
        this._database = await PostgresContainer.CreateDatabase();

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Nakama"] = this._database.ConnectionString
            })
            .Build();

        this._writeSessions = new NhWriteSessionFactory(configuration);
    }

    [TearDown]
    public async Task InfrastructureTearDown()
    {
        this._writeSessions.Dispose();
        await PostgresContainer.DropDatabase(this._database);
    }
}
