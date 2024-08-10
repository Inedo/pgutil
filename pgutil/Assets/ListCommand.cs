using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class AssetsCommand
    {
        private sealed class ListCommand : IConsoleCommand
        {
            public static string Name => "list";
            public static string Description => "Lists assets at the specified path";
            public static string Examples => """
                  $> pgutil assets list --feed=development-assets

                For more information, see: https://docs.inedo.com/docs/proget/reference-api/proget-api-assets/folder-endpoints/proget-api-assets-folders-list
                """;

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<PathOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetAssetDirectoryClient();
                var items = (await client.ListContentsAsync(context.GetOptionOrDefault<PathOption>(), cancellationToken: cancellationToken).ToListAsync())
                    .OrderByDescending(i => i.Directory)
                    .ThenBy(i => i.Name);

                foreach (var item in items)
                {
                    if (item.Size.HasValue)
                        CM.WriteLine($"{item.Name,-50} {item.Size,14}");
                    else
                        CM.WriteLine(ConsoleColor.Blue, $"{item.Name,-50} {"<DIR>",14}");
                }

                return 0;
            }

            private sealed class PathOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--path";
                public static string Description => "Root path to list items in";
            }
        }
    }
}
