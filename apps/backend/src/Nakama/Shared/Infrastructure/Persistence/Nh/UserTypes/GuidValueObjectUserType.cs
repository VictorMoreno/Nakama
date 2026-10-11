using System.Data;
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using Nakama.Shared.Domain;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.UserTypes;

public sealed class GuidValueObjectUserType<T> : IUserType
    where T : IGuidValueObject
{
    public SqlType[] SqlTypes { get; } = [new(DbType.Guid)];

    public Type ReturnedType => typeof(T);

    public bool IsMutable => false;

    public object? NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
    {
        int index = rs.GetOrdinal(names[0]);
        if (rs.IsDBNull(index))
        {
            return null;
        }

        Guid value = rs.GetGuid(index);
        return (T)Activator.CreateInstance(typeof(T), value)!;
    }

    public void NullSafeSet(DbCommand cmd, object? value, int index, ISessionImplementor session)
    {
        DbParameter parameter = cmd.Parameters[index];
        parameter.Value = value is null ? DBNull.Value : ((IGuidValueObject)value).Value;
    }

    public object? DeepCopy(object? value) => value;

    public object? Replace(object? original, object? target, object owner) => original;

    public object? Assemble(object cached, object owner) => cached;

    public object? Disassemble(object? value) => value;

    public new bool Equals(object? x, object? y) => object.Equals(x, y);

    public int GetHashCode(object x) => x.GetHashCode();
}
