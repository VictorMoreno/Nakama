using NHibernate;

namespace Nakama.Shared.Infrastructure.Persistence;

public interface ISessionFactory
{
    ISession OpenSession();
}
