using System.Text.Json.Serialization;

namespace Inedo.ProGet.UniversalPackages;

[JsonSerializable(typeof(RegisteredUniversalPackage[]))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, WriteIndented = true)]
internal sealed partial class UniversalPackageJsonContext : JsonSerializerContext
{
}
