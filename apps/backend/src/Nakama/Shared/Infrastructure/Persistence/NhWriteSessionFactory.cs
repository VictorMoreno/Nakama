using Microsoft.Extensions.Configuration;
using NHibernate;

namespace Nakama.Shared.Infrastructure.Persistence;

public sealed class NhWriteSessionFactory(IConfiguration configuration) : IWriteSessionFactory, IDisposable
{
    private readonly Lazy<NHibernate.ISessionFactory> _sessionFactory =
        new(() => NhSessionFactoryBuilder.Build(configuration, "Nakama"));

    public ISession OpenSession() => this._sessionFactory.Value.OpenSession();

    public void Dispose()
    {
        if (this._sessionFactory.IsValueCreated)
        {
            this._sessionFactory.Value.Dispose();
        }
    }
}
