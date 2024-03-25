using System.Text.Json;

namespace PgUtil.Config;

internal sealed record class PgUtilConfig(PgUtilSource[] Sources)
{
    public static string ConfigFilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "pgutil", "pgutil.config");
    public static PgUtilConfig Instance { get; } = Load(ConfigFilePath);

    public static PgUtilConfig Load(string fileName)
    {
        try
        {
            using var stream = File.OpenRead(fileName);
            return JsonSerializer.Deserialize(stream, ConfigJsonContext.Default.PgUtilConfig)?.Unobfuscate() ?? new PgUtilConfig([]);
        }
        catch
        {
            return new PgUtilConfig([]);
        }
    }
    public void Save(string fileName)
    {
        var dir = Path.GetDirectoryName(fileName);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);
        using var stream = File.Create(fileName);
        JsonSerializer.Serialize(stream, this, ConfigJsonContext.Default.PgUtilConfig);
    }

    private PgUtilConfig Unobfuscate()
    {
        if (this.Sources.Any(s => s.EncryptedPassword is not null || s.EncryptedToken is not null))
            return this with { Sources = [.. this.Sources.Select(s => s.Unobfuscate())] };
        else
            return this;
    }
}
