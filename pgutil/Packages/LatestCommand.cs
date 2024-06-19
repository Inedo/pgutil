using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class PackagesCommand
    {
        private sealed class LatestCommand : IConsoleCommand
        {
            public static string Name => "list";
            public static string Description => "Display latest versions of packages in a feed";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<PackageNameOption>(false)
                    .WithOption<StableOnlyFlag>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();
                var feed = context.GetFeedName();

                _ = TryGetPackageName(context, out _, out var name, out var group);

                await foreach (var p in client.ListLatestPackagesAsync(feed, name, group, context.HasFlag<StableOnlyFlag>(), cancellationToken))
                    Console.WriteLine($"{p.Name} {p.Version}");

                return 0;
            }

            private sealed class StableOnlyFlag : IConsoleFlagOption
            {
                public static string Name => "--stable";
                public static string Description => "List only latest stable versions (do not include prerelease versions)";
            }
        }
    }
}
