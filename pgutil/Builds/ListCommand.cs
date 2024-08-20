using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class BuildsCommand
    {
        private sealed class ListCommand : IConsoleCommand
        {
            public static string Name => "list";
            public static string Description => "Lists builds in a project";
            public static string Examples => """
                  >$ pgutil builds list --project=newApplication

                For more information, see: https://docs.inedo.com/docs/proget/reference-api/proget-api-sca/builds/proget-api-sca-builds-list
                """;

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<SourceOption>()
                    .WithOption<ApiKeyOption>()
                    .WithOption<UserNameOption>()
                    .WithOption<PasswordOption>()
                    .WithOption<ProjectOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();

                await foreach (var build in client.ListBuildsAsync(context.GetOption<ProjectOption>(), cancellationToken))
                    CM.WriteLine(build.Active ? ConsoleColor.Gray : ConsoleColor.DarkGray, build.Version);

                return 0;
            }
        }
    }
}
