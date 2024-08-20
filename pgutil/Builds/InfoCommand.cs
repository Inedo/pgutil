using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class BuildsCommand
    {
        private sealed class InfoCommand : IConsoleCommand
        {
            public static string Name => "info";
            public static string Description => "Shows information about a build";
            public static string Examples => """
                  >$ pgutil builds info --build=4.5.0 --project=newApplication

                For more information, see: https://docs.inedo.com/docs/proget/reference-api/proget-api-sca/builds/proget-api-sca-builds-get
                """;

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<SourceOption>()
                    .WithOption<ApiKeyOption>()
                    .WithOption<UserNameOption>()
                    .WithOption<PasswordOption>()
                    .WithOption<ProjectOption>()
                    .WithOption<BuildOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();
                var build = await client.GetBuildAsync(context.GetOption<ProjectOption>(), context.GetOption<BuildOption>(), cancellationToken);

                CM.WriteLine(build.Active ? ConsoleColor.Gray : ConsoleColor.DarkGray, $"Build {build.Version}");

                if (build.Packages?.Length > 0)
                {
                    Console.WriteLine("Packages:");
                    foreach (var p in build.Packages)
                        Console.WriteLine($"  {p.PUrl}");
                }

                if (build.Comments?.Length > 0)
                {
                    Console.WriteLine("Comments:");
                    foreach (var c in build.Comments)
                        Console.WriteLine($"{c.Number} - {c.Comment}");
                }

                return 0;
            }
        }
    }
}
