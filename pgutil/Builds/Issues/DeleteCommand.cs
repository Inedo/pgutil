using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class BuildsCommand
    {
        private sealed partial class IssuesCommand
        {
            private sealed class DeleteCommand : IConsoleCommand
            {
                public static string Name => "delete";
                public static string Description => "Deletes an issue";
                public static string Examples => """
                      >$ pgutil builds issues delete --project=myProject --build=1.2.3 --number=4

                    For more information, see: https://docs.inedo.com/docs/proget/reference-api/proget-api-sca/issues/proget-api-sca-issues-delete
                    """;

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<NumberOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    await client.DeleteIssueAsync(context.GetOption<ProjectOption>(), context.GetOption<BuildOption>(), context.GetOption<NumberOption, int>(), cancellationToken);
                    Console.WriteLine("Issue deleted.");

                    return 0;
                }
            }
        }
    }
}
