using System.Data;
using NHibernate.Dialect;

namespace Nakama.Shared.Infrastructure.Persistence.Nh;

public sealed class NakamaPostgreSqlDialect : PostgreSQLDialect
{
    public NakamaPostgreSqlDialect()
    {
        this.RegisterColumnType(DbType.Guid, "uuid");
    }
}
