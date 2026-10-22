using Microsoft.Extensions.Configuration;
using Nakama.Shared.Infrastructure.Persistence;

namespace Test.Modules.Shared.Infrastructure;

[Category("Infrastructure")]
public abstract class InfrastructureTestCase
{
    private readonly List<NhUnitOfWork> _unitsOfWork = [];

    private TestDatabase _database = null!;

    private NhSessionFactory _sessionFactory = null!;

    protected INhUnitOfWork UnitOfWork { get; private set; } = null!;

    protected INhUnitOfWorkFactory UnitOfWorkFactory { get; private set; } = null!;

    protected async Task Execute(string sql) => await PostgresContainer.Run(this._database, sql);

    protected INhUnitOfWork NewUnitOfWork()
    {
        NhUnitOfWork unitOfWork = new(this._sessionFactory);
        this._unitsOfWork.Add(unitOfWork);

        return unitOfWork;
    }

    protected async Task Persist()
    {
        await this.UnitOfWork.SaveChanges(CancellationToken.None);
        this.UnitOfWork.Session.Clear();
    }

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

        this._sessionFactory = new NhSessionFactory(configuration);
        this.UnitOfWorkFactory = new NhUnitOfWorkFactory(this._sessionFactory);
        this.UnitOfWork = this.NewUnitOfWork();
    }

    [TearDown]
    public async Task InfrastructureTearDown()
    {
        foreach (NhUnitOfWork unitOfWork in this._unitsOfWork)
        {
            await unitOfWork.DisposeAsync();
        }

        this._unitsOfWork.Clear();
        this._sessionFactory.Dispose();
        await PostgresContainer.DropDatabase(this._database);
    }
}
