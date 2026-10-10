using System.Data;
using System.Data.Common;
using System.Text.Json;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.UserTypes;

public sealed class JsonUserType<T> : IUserType
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public SqlType[] SqlTypes { get; } = [new(DbType.String)];

    public Type ReturnedType => typeof(T);

    public bool IsMutable => true;

    public object? NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
    {
        var index = rs.GetOrdinal(names[0]);
        if (rs.IsDBNull(index))
        {
            return default(T);
        }

        var json = rs.GetString(index);
        return JsonSerializer.Deserialize<T>(json, SerializerOptions);
    }

    public void NullSafeSet(DbCommand cmd, object? value, int index, ISessionImplementor session)
    {
        var parameter = cmd.Parameters[index];
        parameter.Value = value is null ? DBNull.Value : JsonSerializer.Serialize((T)value, SerializerOptions);
    }

    public object? DeepCopy(object? value) =>
        value is null ? default(T) : JsonSerializer.Deserialize<T>(JsonSerializer.Serialize((T)value, SerializerOptions), SerializerOptions);

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

        return JsonSerializer.Serialize(x, SerializerOptions) == JsonSerializer.Serialize(y, SerializerOptions);
    }

    public int GetHashCode(object x) => JsonSerializer.Serialize(x, SerializerOptions).GetHashCode();
}
