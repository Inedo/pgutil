using ConsoleMan;
using Inedo.ProGet.AssetDirectories;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class AssetsCommand
    {
        private sealed partial class FolderCommand
        {
            private sealed class ImportCommand : IConsoleCommand
            {
                public static string Name => "import";
                public static string Description => "Import the contents of a zip or tar archive to a folder";

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<PathOption>()
                        .WithOption<FileOption>()
                        .WithOption<OverwriteFlag>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetAssetDirectoryClient();
                    var fileName = context.GetOption<FileOption>();
                    if (!File.Exists(fileName))
                    {
                        CM.WriteError<FileOption>($"{fileName} not found.");
                        return -1;
                    }

                    ArchiveFormat? format = Path.GetExtension(fileName).ToLowerInvariant() switch
                    {
                        ".zip" => ArchiveFormat.Zip,
                        ".tar.gz" or ".tgz" => ArchiveFormat.TarGzip,
                        _ => null
                    };

                    if (!format.HasValue)
                    {
                        CM.WriteError<FileOption>("Invalid archive type: must be .zip, .tar.gz, or .tgz");
                        return -1;
                    }

                    var path = context.GetOptionOrDefault<PathOption>();

                    CM.WriteLine("Importing ", new TextSpan(fileName, ConsoleColor.White), " to ", new TextSpan(path ?? "/", ConsoleColor.White), "...");

                    using var source = File.Open(
                        fileName,
                        new FileStreamOptions
                        {
                            Access = FileAccess.Read,
                            Mode = FileMode.Open,
                            Options = FileOptions.SequentialScan | FileOptions.Asynchronous
                        }
                    );

                    await client.ImportArchiveAsync(
                        source,
                        format.GetValueOrDefault(),
                        path,
                        context.HasFlag<OverwriteFlag>(),
                        cancellationToken
                    );

                    CM.WriteLine("Archive imported!");
                    return 0;
                }

                private sealed class FileOption : IConsoleOption
                {
                    public static bool Required => true;
                    public static string Name => "--file";
                    public static string Description => "Archive file (.zip or .tar.gz) to import";
                }

                private sealed class PathOption : IConsoleOption
                {
                    public static bool Required => false;
                    public static string Name => "--path";
                    public static string Description => "Path of folder to import to";
                }

                private sealed class OverwriteFlag : IConsoleFlagOption
                {
                    public static string Name => "--overwrite";
                    public static string Description => "Overwrite already existing items in the asset directory";
                }
            }
        }
    }
}
