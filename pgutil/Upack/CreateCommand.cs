using System.IO.Compression;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class UpackCommand
    {
        private sealed class CreateCommand : IConsoleCommand
        {
            public static string Name => "create";
            public static string Description => "Creates a universal package";
            public static string Examples => """
                  $> $ pgutil upack create --name=my-package --version=1.2.3 --source-directory=.\package-files\my-package --target-directory=.\universal-packages
                  $> $ pgutil upack create --manifest=.\package-files\my-package\upack.json --source-directory=.\package-files\my-package --target-directory=.\universal-packages

                For more information, see: https://docs.inedo.com/docs/proget/feeds/universal#upack-create
                """;

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<ManifestOption>()
                    .WithOption<NameOption>()
                    .WithOption<VersionOption>()
                    .WithOption<SourceDirectoryOption>()
                    .WithOption<TargetDirectoryOption>()
                    .WithOption<OverwriteFlag>();
            }

            public static Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var manifestPath = context.GetOptionOrDefault<ManifestOption>();
                var name = context.GetOptionOrDefault<NameOption>();
                var version = context.GetOptionOrDefault<VersionOption>();

                JsonObject manifest;
                if (!string.IsNullOrEmpty(manifestPath))
                {
                    if (!File.Exists(manifestPath))
                        throw new PgUtilException($"{manifestPath} not found.");

                    using var manifestStream = File.OpenRead(manifestPath);
                    JsonNode? node;
                    try
                    {
                        node = JsonNode.Parse(manifestStream);
                    }
                    catch (Exception ex)
                    {
                        throw new PgUtilException($"Invalid JSON in manifest file: {ex.Message}");
                    }

                    if (node is not JsonObject obj)
                        throw new PgUtilException("Expected JSON object at root of manifest file.");

                    manifest = obj;
                }
                else
                {
                    manifest = [];
                }

                if (!string.IsNullOrEmpty(name))
                {
                    int index = name.LastIndexOf('/');
                    if (index >= 0)
                    {
                        var g = name[0..index];
                        var n = name[(index + 1)..];

                        if (string.IsNullOrEmpty(n))
                        {
                            CM.WriteError<NameOption>("Invalid name specification");
                            return Task.FromResult(-1);
                        }

                        manifest["group"] = g;
                        manifest["name"] = n;
                    }
                    else
                    {
                        manifest["group"] = string.Empty;
                        manifest["name"] = name;
                    }
                }

                if (!string.IsNullOrEmpty(version))
                    manifest["version"] = version;

                if (manifest["name"] is null || manifest["version"] is null)
                    throw new PgUtilException("Package name and version must be specified either in a manifest file or using the --name and --version arguments");

                var fileName = Path.Combine(context.GetOption<TargetDirectoryOption>(), $"{manifest["name"]}-{manifest["version"]}.upack");
                if (!context.HasFlag<OverwriteFlag>() && File.Exists(fileName))
                    throw new PgUtilException($"{fileName} already exists and --overwrite was not specified");

                CM.WriteLine($"Creating {fileName}...");

                using var zipStream = File.Create(fileName);
                using var zip = new ZipArchive(zipStream, ZipArchiveMode.Create);
                var manifestEntry = zip.CreateEntry("upack.json");
                using (var upackJsonStream = manifestEntry.Open())
                {
                    using var writer = new Utf8JsonWriter(upackJsonStream, new JsonWriterOptions { Indented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });
                    manifest.WriteTo(writer);
                }

                var srcPath = Path.GetFullPath(context.GetOptionOrDefault<SourceDirectoryOption>() ?? string.Empty);
                if (!Directory.Exists(srcPath))
                {
                    CM.WriteError<SourceDirectoryOption>("Source directory not found");
                    return Task.FromResult(-1);
                }

                if (manifestPath is not null)
                    manifestPath = Path.GetFullPath(manifestPath);

                fileName = Path.GetFullPath(fileName);

                foreach (var srcFile in Directory.EnumerateFiles(context.GetOptionOrDefault<SourceDirectoryOption>() ?? string.Empty, "*", SearchOption.AllDirectories))
                {
                    if (srcFile.Equals(manifestPath, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                        continue;
                    if (srcFile.Equals(fileName, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                        continue;

                    var itemPath = Path.Combine("package", Path.GetRelativePath(srcPath, srcFile)).Replace('\\', '/');
                    zip.CreateEntryFromFile(srcFile, itemPath);
                }

                CM.WriteLine($"{fileName} created.");

                return Task.FromResult(0);
            }

            private sealed class ManifestOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--manifest";
                public static string Description => "upack.json file to use as the package manifest";
            }

            private sealed class NameOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--name";
                public static string Description => "Name of the package; this will override what is specified in the manifest";
            }

            private sealed class VersionOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--version";
                public static string Description => "Version of the package; this will override what is specified in the manifest";
            }

            private sealed class SourceDirectoryOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--source-directory";
                public static string Description => "Package content root directory";
            }

            private sealed class TargetDirectoryOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--target-directory";
                public static string Description => "Directory where package file will be written";
            }

            private sealed class OverwriteFlag : IConsoleFlagOption
            {
                public static string Name => "--overwrite";
                public static string Description => "Overwrite package in target directory";
            }
        }
    }
}
