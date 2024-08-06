using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class PackagesCommand
    {
        private sealed class VersionsCommand : IConsoleCommand
        {
            public static string Name => "versions";
            public static string Description => "Display all versions of packages in a feed";
            public static string Examples => """
                  $> pgutil packages versions --package=Newtonsoft.Json --feed=approved-nuget
                  $> pgutil packages versions --package=@babel/runtime --feed=public-npm --stable=true
                  $> pgutil packages versions --package=django --feed=private-pypi

                For more information, see: https://docs.inedo.com/docs/proget/reference-api/proget-api-packages/proget-api-packages-list-versions
                """;

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<PackageNameOption>(false)
                    .WithOption<PackageVersionOption>(false);
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();
                var feed = context.GetFeedName();

                _ = TryGetPackageName(context, out _, out var name, out var group);

                await foreach (var p in client.ListPackagesAsync(feed, name, group, context.GetOptionOrDefault<PackageVersionOption>(), cancellationToken))
                    Console.WriteLine($"{p.Name} {p.Version}");

                return 0;
            }
        }
    }
}
