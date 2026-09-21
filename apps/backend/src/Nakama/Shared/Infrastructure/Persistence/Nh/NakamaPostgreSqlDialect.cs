using System.Data;
using NHibernate.Dialect;

namespace Nakama.Shared.Infrastructure.Persistence.Nh;

/// <summary>
/// NHibernate's stock <see cref="PostgreSQLDialect"/> has no DDL type-name mapping for
/// <see cref="DbType.Guid"/> - only relevant to schema generation (<c>SchemaExport</c>), never
/// to normal reads/writes, since those go through each UserType's own NullSafeGet/NullSafeSet.
/// Registering it here maps every Guid-backed column (<see cref="GuidValueObjectUserType{T}"/>)
/// onto Postgres' native <c>uuid</c> type when generating CREATE TABLE scripts.
/// </summary>
public sealed class NakamaPostgreSqlDialect : PostgreSQLDialect
{
    public NakamaPostgreSqlDialect()
    {
        RegisterColumnType(DbType.Guid, "uuid");
    }
}
