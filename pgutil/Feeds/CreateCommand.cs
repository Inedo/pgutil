using ConsoleMan;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class FeedsCommand
    {
        private sealed class CreateCommand : IConsoleCommand
        {
            public static string Name => "create";
            public static string Description => "Creates a feed";
            public static string Examples => """
                  $> pgutil feeds create --name=public-nuget --type=NuGet

                For more information, see: https://docs.inedo.com/docs/proget/reference-api/feeds/proget-api-feeds/proget-api-feeds-create
                """;

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<NameOption>()
                    .WithOption<TypeOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();
                var feedName = context.GetOption<NameOption>();
                var feedType = context.GetOption<TypeOption>();

                CM.WriteLine("Creating ", new TextSpan(feedType, ConsoleColor.Blue), " feed: ", new TextSpan(feedName, ConsoleColor.White), "...");
                _ = await client.CreateFeedAsync(feedName, feedType, cancellationToken);
                Console.WriteLine("Feed created.");
                return 0;
            }

            private sealed class NameOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--name";
                public static string Description => "Name of the new feed to create";
            }

            private sealed class TypeOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--type";
                public static string Description => "Type of the feed to create";
                public static string[] ValidValues => ["NuGet", "Chocolatey", "npm", "Bower", "Maven", "Universal", "PowerShell", "Docker", "RubyGems", "VSIX", "Debian", "PyPI", "Helm", "RPM", "Conda", "APK", "CRAN"];
            }
        }
    }
}
