using ConsoleMan;

namespace PgUtil;

partial class Program
{
    partial class ContainersCommand
    {
        private sealed partial class TagsCommand
        {
            private sealed class DeleteCommand : IConsoleCommand
            {
                public static string Name => "delete";
                public static string Description => "Deletes a tag from an image in ProGet";
                public static string Examples => """
                      $> pgutil containers tags delete --feed=internal-docker --repo=nginx --target=1.82
                      $> pgutil containers tags delete --feed=internal-docker --repo=proget --target=23.0.0-ci.1
                    """;

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<RepoOption>()
                        .WithOption<TagOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    var feed = context.GetFeedName();
                    var repo = context.GetOption<RepoOption>();
                    var tag = context.GetOption<TagOption>();

                    CM.WriteLine("Deleting tag ", new TextSpan(tag, ConsoleColor.Blue), " from ", new TextSpan(repo, ConsoleColor.White), "...");
                    await client.DeleteContainerImageTagAsync(feed, repo, tag, cancellationToken);
                    CM.WriteLine("Tag deleted.");
                    return 0;
                }

                private sealed class RepoOption : IConsoleOption
                {
                    public static bool Required => true;
                    public static string Name => "--repo";
                    public static string Description => "Repository of the image";
                }

                private sealed class TagOption : IConsoleOption
                {
                    public static bool Required => true;
                    public static string Name => "--tag";
                    public static string Description => "Tag to delete from the repository";
                }
            }
        }
    }
}
