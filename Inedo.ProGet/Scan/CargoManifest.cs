using System.Text.Json.Serialization;
using Tomlyn.Serialization;

namespace Inedo.DependencyScan;

internal sealed class CargoManifest
{
    public required CargoManifestPackage Package { get; init; }
}

internal sealed class CargoManifestPackage
{
    public required string Name { get; init; }
}

internal sealed class CargoLockFile
{
    public CargoLockFilePackage[]? Package { get; init; }
}

internal sealed record class CargoLockFilePackage(string? Name, string? Version);

[TomlSerializable(typeof(CargoLockFile))]
[TomlSerializable(typeof(CargoManifest))]
[TomlSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PreferredObjectCreationHandling = JsonObjectCreationHandling.Replace)]
internal sealed partial class CargoTomlContext : TomlSerializerContext
{
}
