using Nakama.Shared.Domain;
using NHibernate;

namespace Nakama.Shared.Infrastructure.Persistence;

public interface INhUnitOfWork : IUnitOfWork
{
    ISession Session { get; }

    void EnsureTransaction();

    Task<TResult> Read<TResult>(Func<ISession, Task<TResult>> read);
}
