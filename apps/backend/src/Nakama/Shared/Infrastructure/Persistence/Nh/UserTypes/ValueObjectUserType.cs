using System.Data;
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using Nakama.Shared.Domain;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.UserTypes;

/// <summary>
/// Maps a string-wrapping identity value object (e.g. <c>ChatbotKey</c>) directly onto a
/// varchar column, round-tripping through its <c>(string)</c> constructor - every such VO is a
/// positional record struct, so <see cref="Activator.CreateInstance(Type, object?[])"/> works
/// generically without per-type mapping code.
/// </summary>
public sealed class ValueObjectUserType<T> : IUserType
    where T : IStringValueObject
{
    public SqlType[] SqlTypes { get; } = [new SqlType(DbType.String)];

    public Type ReturnedType => typeof(T);

    public bool IsMutable => false;

    public object? NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
    {
        var index = rs.GetOrdinal(names[0]);
        if (rs.IsDBNull(index))
        {
            return null;
        }

        var value = rs.GetString(index);
        return (T)Activator.CreateInstance(typeof(T), value)!;
    }

    public void NullSafeSet(DbCommand cmd, object? value, int index, ISessionImplementor session)
    {
        var parameter = (DbParameter)cmd.Parameters[index];
        parameter.Value = value is null ? DBNull.Value : ((IStringValueObject)value).Value;
    }

    public object? DeepCopy(object? value) => value;

    public object? Replace(object? original, object? target, object owner) => original;

    /// <summary>Value object is immutable, so second-level-cache assemble/disassemble is a passthrough.</summary>
    public object? Assemble(object cached, object owner) => cached;

    /// <summary>Value object is immutable, so second-level-cache assemble/disassemble is a passthrough.</summary>
    public object? Disassemble(object? value) => value;

    public new bool Equals(object? x, object? y) => object.Equals(x, y);

    public int GetHashCode(object x) => x.GetHashCode();
}
