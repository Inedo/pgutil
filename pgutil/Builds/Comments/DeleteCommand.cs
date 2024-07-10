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
                public static string Description => "Delete a comment";

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
