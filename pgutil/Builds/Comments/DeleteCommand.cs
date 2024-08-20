using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class BuildsCommand
    {
        private sealed partial class CommentsCommand
        {
            private sealed class DeleteCommand : IConsoleCommand
            {
                public static string Name => "delete";
                public static string Description => "Deletes a comment";
                public static string Examples => """
                      >$ pgutil builds comments delete --project=myProject --build=1.2.3 --number=4

                    For more information, see: https://docs.inedo.com/docs/proget/reference-api/proget-api-sca/issues/proget-api-sca-comments-delete
                    """;

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<NumberOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    await client.DeleteCommentAsync(context.GetOption<ProjectOption>(), context.GetOption<BuildOption>(), context.GetOption<NumberOption, int>(), cancellationToken);
                    Console.WriteLine("Comment deleted.");

                    return 0;
                }
            }
        }
    }
}
