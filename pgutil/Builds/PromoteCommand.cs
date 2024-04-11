using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class BuildsCommand
    {
        private sealed class PromoteCommand : IConsoleCommand
        {
            public static string Name => "promote";
            public static string Description => "Promotes a build to another stage";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<SourceOption>()
                    .WithOption<ApiKeyOption>()
                    .WithOption<UserNameOption>()
                    .WithOption<PasswordOption>()
                    .WithOption<ProjectOption>()
                    .WithOption<BuildOption>()
                    .WithOption<StageOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();

                var project = context.GetOption<ProjectOption>();
                var build = context.GetOption<BuildOption>();
                var stage = context.GetOption<StageOption>();

                CM.WriteLine("Promoting ", new TextSpan($"{project} {build}", ConsoleColor.White), " to ", new TextSpan(stage, ConsoleColor.White), "...");
                var results = await client.PromoteBuildAsync(project, build, stage, cancellationToken);
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

            private sealed class ProjectOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--project";
                public static string Description => "Project containing the build to promote";
            }

            private sealed class BuildOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--build";
                public static string Description => "Build number to promote";
            }

            private sealed class StageOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--stage";
                public static string Description => "Stage to promote build to";
            }
        }
    }
}
