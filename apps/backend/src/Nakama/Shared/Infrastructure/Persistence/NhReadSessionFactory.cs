using Microsoft.Extensions.Configuration;
using NHibernate;

namespace Nakama.Shared.Infrastructure.Persistence;

public sealed class NhReadSessionFactory(IConfiguration configuration) : IReadSessionFactory
{
    private readonly Lazy<NHibernate.ISessionFactory> _sessionFactory =
        new(() => NhSessionFactoryBuilder.Build(configuration, "NakamaRead"));

    public ISession OpenSession() => this._sessionFactory.Value.OpenSession();
}
