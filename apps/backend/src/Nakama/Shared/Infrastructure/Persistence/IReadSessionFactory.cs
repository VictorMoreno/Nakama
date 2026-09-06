using NHibernate;

namespace Nakama.Shared.Infrastructure.Persistence;

/// <summary>
/// Wraps the read-side NHibernate <see cref="NHibernate.ISessionFactory"/> that projections write into
/// and queries read from. Registered as a singleton, same rationale as
/// <see cref="IWriteSessionFactory"/>.
/// </summary>
public interface IReadSessionFactory : ISessionFactory
{
}
