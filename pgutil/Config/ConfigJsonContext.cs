using System.Text.Json.Serialization;

namespace PgUtil.Config;

[JsonSerializable(typeof(PgUtilConfig))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal sealed partial class ConfigJsonContext : JsonSerializerContext
{
}
