using NHibernate;

namespace Nakama.Shared.Infrastructure.Persistence;

/// <summary>
/// Shared shape behind <see cref="IWriteSessionFactory"/>/<see cref="IReadSessionFactory"/> -
/// lets <c>NhRepositoryBase&lt;TEntity, TId&gt;</c> (Shared/Infrastructure/Persistence/Nh) take
/// either one without depending on the write/read-specific DI token itself.
/// </summary>
public interface ISessionFactory
{
    ISession OpenSession();
}
