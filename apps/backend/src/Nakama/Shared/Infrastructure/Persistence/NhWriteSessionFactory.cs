using Microsoft.Extensions.Configuration;
using NHibernate;

namespace Nakama.Shared.Infrastructure.Persistence;

/// <summary>
/// Registered as a singleton. Building an <see cref="NHibernate.ISessionFactory"/> is expensive,
/// so it happens once, and lazily on first use (not at DI-container-build time / app startup) so
/// the app can start and compile/run without a reachable Postgres instance.
/// </summary>
public sealed class NhWriteSessionFactory(IConfiguration configuration) : IWriteSessionFactory
{
    private readonly Lazy<NHibernate.ISessionFactory> _sessionFactory =
        new(() => NhSessionFactoryBuilder.Build(configuration, "NakamaWrite"));

    public ISession OpenSession() => this._sessionFactory.Value.OpenSession();
}
