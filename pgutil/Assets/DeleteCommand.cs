using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class AssetsCommand
    {
        private sealed class DeleteCommand : IConsoleCommand
        {
            public static string Name => "delete";
            public static string Description => "Deletes a file or subdirectory in an asset directory";
            public static string Examples => """
                  $> pgutil assets delete --feed=development-assets --path=test-files/test-package.tgz

                  $> pgutil assets delete --feed=production-assets --path=temp-folder

                  $> pgutil assets delete --feed=test-assets --path=test-files --force

                For more information, see:
                  * https://docs.inedo.com/docs/proget/reference-api/proget-api-assets/file-endpoints/proget-api-assets-files-delete
                  * https://docs.inedo.com/docs/proget/reference-api/proget-api-assets/folder-endpoints/proget-api-assets-folders-delete
                """;

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<PathOption>()
                    .WithOption<ForceOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetAssetDirectoryClient();
                var path = context.GetOption<PathOption>();
                CM.WriteLine("Deleting ", new TextSpan(path, ConsoleColor.White), " from ", client.AssetDirectoryName, "...");
                await client.DeleteItemAsync(path, context.HasFlag<ForceOption>(), cancellationToken);
                CM.WriteLine(new TextSpan(path, ConsoleColor.White), " deleted.");
                return 0;
            }

            private sealed class PathOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--path";
                public static string Description => "Path of item to delete. It is not an error if the item doesn't exist";
            }

            private sealed class ForceOption : IConsoleFlagOption
            {
                public static string Name => "--force";
                public static string Description => "Deletes a subdirectory even if it is not empty";
            }
        }
    }
}
