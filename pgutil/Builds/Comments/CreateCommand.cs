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
                public static string Description => "Creates a comment";
                public static string Examples => """
                      >$ pgutil builds comments create --project=newApplication --build=1.0.1 --comment="Checked for errors on 01/01"

                    For more information, see: https://docs.inedo.com/docs/proget/reference-api/proget-api-sca/comments/proget-api-sca-comments-create
                    """;

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
