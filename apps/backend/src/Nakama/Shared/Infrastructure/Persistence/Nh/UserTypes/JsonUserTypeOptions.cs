using System.Text.Json;

namespace Nakama.Shared.Infrastructure.Persistence.Nh.UserTypes;

internal static class JsonUserTypeOptions
{
    public static readonly JsonSerializerOptions Serializer = new(JsonSerializerDefaults.Web);
}
