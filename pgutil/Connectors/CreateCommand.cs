using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal sealed partial class Program
{
    private sealed partial class ConnectorsCommand
    {
        private sealed class CreateCommand : IConsoleCommand
        {
            public static string Name => "create";
            public static string Description => "Creates a connector";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<NameOption>()
                    .WithOption<TypeOption>()
                    .WithOption<UrlOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();

                var connector = new ProGetConnector
                {
                    Name = context.GetOption<NameOption>(),
                    FeedType = context.GetOption<TypeOption>(),
                    Url = context.GetOption<UrlOption>()
                };

                CM.WriteLine("Creating ", new TextSpan(connector.FeedType, ConsoleColor.Blue), " feed ", new TextSpan(connector.Name, ConsoleColor.White), " to ", connector.Url, "...");
                await client.CreateConnectorAsync(connector, cancellationToken);
                Console.WriteLine("Connector created.");
                return 0;
            }

            private sealed class NameOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--name";
                public static string Description => "Name of the new connector";
            }

            private sealed class TypeOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--type";
                public static string Description => "Type of the connector to create";
                public static string[] ValidValues => ["NuGet", "Chocolatey", "npm", "Maven", "Universal", "PowerShell", "Docker", "RubyGems", "VSIX", "Debian", "PyPI", "Helm", "RPM", "Conda", "APK", "CRAN"];
            }

            private sealed class UrlOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--url";
                public static string Description => "Remote URL of the connector";
            }
        }
    }
}
