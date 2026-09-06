using NHibernate;

namespace Nakama.Shared.Infrastructure.Persistence;

/// <summary>
/// Wraps the write-side NHibernate <see cref="NHibernate.ISessionFactory"/>. Registered as a singleton -
/// building a session factory is expensive and must happen once at startup, never per request.
/// </summary>
public interface IWriteSessionFactory : ISessionFactory
{
}
