using System.Text.Json.Serialization;

namespace Inedo.ProGet;

[JsonSerializable(typeof(ProGetFeed))]
[JsonSerializable(typeof(PackageStatus))]
[JsonSerializable(typeof(ProGetInstanceHealth))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class ProGetApiJsonContext : JsonSerializerContext
{
}
