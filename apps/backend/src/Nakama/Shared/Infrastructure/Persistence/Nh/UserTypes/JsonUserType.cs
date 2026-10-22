using System.Data;
using System.Data.Common;
using System.Text.Json;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.UserTypes;

public sealed class JsonUserType<T> : IUserType
{
    public SqlType[] SqlTypes { get; } = [new(DbType.String)];

    public Type ReturnedType => typeof(T);

    public bool IsMutable => true;

    public object? NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
    {
        int index = rs.GetOrdinal(names[0]);
        if (rs.IsDBNull(index))
        {
            return default(T);
        }

        string json = rs.GetString(index);
        return JsonSerializer.Deserialize<T>(json, JsonUserTypeOptions.Serializer);
    }

    public void NullSafeSet(DbCommand cmd, object? value, int index, ISessionImplementor session)
    {
        DbParameter parameter = cmd.Parameters[index];
        parameter.Value = value is null ? DBNull.Value : JsonSerializer.Serialize((T)value, JsonUserTypeOptions.Serializer);
    }

    public object? DeepCopy(object? value) =>
        value is null ? default(T) : JsonSerializer.Deserialize<T>(JsonSerializer.Serialize((T)value, JsonUserTypeOptions.Serializer), JsonUserTypeOptions.Serializer);

    public object? Replace(object? original, object? target, object owner) => this.DeepCopy(original);

    public object? Assemble(object cached, object owner) => this.DeepCopy(cached);

    public object? Disassemble(object? value) => this.DeepCopy(value);

    public new bool Equals(object? x, object? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null || y is null)
        {
            return false;
        }

        return JsonSerializer.Serialize(x, JsonUserTypeOptions.Serializer) == JsonSerializer.Serialize(y, JsonUserTypeOptions.Serializer);
    }

    public int GetHashCode(object x) => JsonSerializer.Serialize(x, JsonUserTypeOptions.Serializer).GetHashCode();
}
