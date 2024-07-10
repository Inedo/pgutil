using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class BuildsCommand
    {
        private sealed partial class CommentsCommand
        {
            private sealed class CreateCommand : IConsoleCommand
            {
                public static string Name => "create";
                public static string Description => "Create a comment";

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<CommentOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();

                    var comment = new BuildCommentCreateInfo
                    {
                        Project = context.GetOption<ProjectOption>(),
                        Version = context.GetOption<BuildOption>(),
                        Comment = context.GetOption<CommentOption>()
                    };

                    await client.CreateCommentAsync(comment, cancellationToken).ConfigureAwait(false);
                    Console.WriteLine("Comment created.");

                    return 0;
                }

                private sealed class CommentOption : IConsoleOption
                {
                    public static bool Required => true;
                    public static string Name => "--comment";
                    public static string Description => "Text of comment to create";
                }
            }
        }
    }
}
