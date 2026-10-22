namespace Nakama.Shared.Infrastructure.Persistence;

public sealed class NhUnitOfWorkFactory(ISessionFactory sessionFactory) : INhUnitOfWorkFactory
{
    public INhUnitOfWork Create() => new NhUnitOfWork(sessionFactory);
}
