using ConsoleMan;
using Inedo.ProGet.AssetDirectories;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class AssetsCommand
    {
        private sealed partial class FolderCommand
        {
            private sealed class ExportCommand : IConsoleCommand
            {
                public static string Name => "export";
                public static string Description => "Export the contents of an asset directory folder to an archive file";

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
                    if (File.Exists(fileName) && !context.HasFlag<OverwriteFlag>())
                    {
                        CM.WriteError<FileOption>($"{fileName} already exists.");
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

                    CM.WriteLine("Exporting ", new TextSpan(path ?? "/", ConsoleColor.White), " to ", new TextSpan(fileName, ConsoleColor.White), "...");

                    try
                    {
                        using var target = File.Open(
                            fileName,
                            new FileStreamOptions
                            {
                                Access = FileAccess.Write,
                                Mode = FileMode.Create,
                                Options = FileOptions.SequentialScan | FileOptions.Asynchronous
                            }
                        );

                        await client.ExportFolderAsync(
                            target,
                            format.GetValueOrDefault(),
                            path,
                            context.HasFlag<RecursiveFlag>(),
                            cancellationToken
                        );

                        CM.WriteLine("Folder exported!");
                        return 0;
                    }
                    catch
                    {
                        try
                        {
                            File.Delete(fileName);
                        }
                        catch
                        {
                        }

                        throw;
                    }
                }

                private sealed class FileOption : IConsoleOption
                {
                    public static bool Required => true;
                    public static string Name => "--file";
                    public static string Description => "Path of output archive file (.zip or .tar.gz)";
                }

                private sealed class PathOption : IConsoleOption
                {
                    public static bool Required => false;
                    public static string Name => "--path";
                    public static string Description => "Path of folder to export";
                }

                private sealed class OverwriteFlag : IConsoleFlagOption
                {
                    public static string Name => "--overwrite";
                    public static string Description => "Overwrite the output archive file if it already exists";
                }

                private sealed class RecursiveFlag : IConsoleFlagOption
                {
                    public static string Name => "--recursive";
                    public static string Description => "Include subfolders and their contents in the archive";
                }
            }
        }
    }
}
