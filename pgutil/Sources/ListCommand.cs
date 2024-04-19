using ConsoleMan;
using PgUtil.Config;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SourcesCommand
    {
        private sealed class ListCommand : IConsoleCommand
        {
            public static string Name => "list";
            public static string Description => "Lists sources stored in the configuration file";

            public static void Configure(ICommandBuilder builder)
            {
            }

            public static Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var sources = PgUtilConfig.Instance.Sources;

                if (sources.Length == 0)
                {
                    CM.WriteLine(ConsoleColor.Yellow, "No sources configured.");
                }
                else
                {
                    /*

1. Default
   https://proget.corp.local/ (api)

2. MyPackages (unapproved-nuget)
   https://othersource.somewhere.dev/ (devbuilds)                     
                     */
                    for (int i = 0; i < sources.Length; i++)
                    {
                        var s = sources[i];

                        CM.Write(ConsoleColor.White, $"{i+1}. {s.Name}");
                        if (!string.IsNullOrEmpty(s.DefaultFeed))
                            CM.Write(new TextSpan(" ("), new TextSpan(s.DefaultFeed, ConsoleColor.White), new TextSpan(" feed)"));

                        Console.WriteLine();
                        Console.Write("   ");
                        var uri = new UriBuilder(s.Url);
                        if (s.Token is not null || s.EncryptedToken is not null)
                            uri.UserName = "api";
                        else if (s.Username is not null)
                            uri.UserName = s.Username;

                        Console.Write(uri.Uri);


                        Console.WriteLine();
                    }
                }

                return Task.FromResult(0);
            }
        }
    }
}
