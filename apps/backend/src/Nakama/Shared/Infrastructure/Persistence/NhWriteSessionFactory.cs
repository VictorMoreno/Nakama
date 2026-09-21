using Microsoft.Extensions.Configuration;
using NHibernate;

namespace Nakama.Shared.Infrastructure.Persistence;

public sealed class NhWriteSessionFactory(IConfiguration configuration) : IWriteSessionFactory
{
    private readonly Lazy<NHibernate.ISessionFactory> _sessionFactory =
        new(() => NhSessionFactoryBuilder.Build(configuration, "Nakama"));

    public ISession OpenSession() => this._sessionFactory.Value.OpenSession();
}
