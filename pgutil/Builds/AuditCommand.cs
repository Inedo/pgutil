using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class BuildsCommand
    {
        private sealed class AuditCommand : IConsoleCommand
        {
            public static string Name => "audit";
            public static string Description => "Analyzes a build and reports issues";

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

                var project = context.GetOption<ProjectOption>();
                var build = context.GetOption<BuildOption>();

                CM.WriteLine("Auditing ", new TextSpan($"{project} {build}", ConsoleColor.White), "...");
                var results = await client.AuditBuildAsync(project, build, cancellationToken);
                if (results.LastAnalyzedDate.HasValue)
                {
                    CM.WriteLine("Analyzed: ", new TextSpan(results.LastAnalyzedDate.Value.ToLocalTime().ToString(), ConsoleColor.White));
                    CM.WriteLine(
                        "Status: ",
                        new TextSpan(
                            results.StatusText,
                            results.StatusCode switch
                            {
                                "N" when results.UnresolvedIssueCount == 0 => ConsoleColor.Blue,
                                "N" => ConsoleColor.Red,
                                "W" => ConsoleColor.DarkYellow,
                                "I" => ConsoleColor.Yellow,
                                _ => ConsoleColor.Green
                            }
                        )
                    );

                    return results.StatusCode switch
                    {
                        "N" when results.UnresolvedIssueCount == 0 => 0,
                        "N" => -1,
                        _ => 0
                    };
                }
                else
                {
                    // this should not happen
                    CM.WriteError("ProGet reported that the build was not analyzed.");
                    return -1;
                }
            }
        }
    }
}
