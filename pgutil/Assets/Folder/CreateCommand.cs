using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class AssetsCommand
    {
        private sealed partial class FolderCommand
        {
            private sealed class CreateCommand : IConsoleCommand
            {
                public static string Name => "create";
                public static string Description => "Creates a folder in an asset directory";

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<PathOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetAssetDirectoryClient();
                    var path = context.GetOption<PathOption>();
                    CM.WriteLine("Creating ", new TextSpan(path, ConsoleColor.White), " on ", client.AssetDirectoryName, "...");
                    await client.CreateDirectoryAsync(path, cancellationToken);
                    CM.WriteLine(new TextSpan(path, ConsoleColor.White), " created!");
                    return 0;
                }

                private sealed class PathOption : IConsoleOption
                {
                    public static bool Required => true;
                    public static string Name => "--path";
                    public static string Description => "Path of folder to create. It is not an error if the folder already exists";
                }
            }
        }
    }
}
