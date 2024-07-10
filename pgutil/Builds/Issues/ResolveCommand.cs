using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class BuildsCommand
    {
        private sealed partial class IssuesCommand
        {
            private sealed class ResolveCommand : IConsoleCommand
            {
                public static string Name => "resolve";
                public static string Description => "Resolves an open issue";

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<NumberOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    await client.ResolveIssueAsync(context.GetOption<ProjectOption>(), context.GetOption<BuildOption>(), context.GetOption<NumberOption, int>(), cancellationToken);
                    Console.WriteLine("Issue resolved.");

                    return 0;
                }
            }
        }
    }
}
