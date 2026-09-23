using ConsoleMan;

namespace PgUtil;

partial class Program
{
    partial class ContainersCommand
    {

        private sealed partial class ListTagsCommand : IConsoleCommand
        {
            public static string Name => "list";

            public static string Description => "List tags for a repository in ProGet";

            public static string Examples => """
              $> pgutil container tags list --feed=internal-docker --repo=nginx
            """;
            public static void Configure(ICommandBuilder builder)
            {
                builder.WithOption<RepoOption>();
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();
                var feed = context.GetFeedName();
                var repo = context.GetOption<RepoOption>();

                CM.WriteLine("Listing tags for ", new TextSpan(repo, ConsoleColor.White), "...");
                var tags = client.ListContainerImageTagsAsync(feed, repo, cancellationToken);

                await foreach(var tag in tags)
                {
                    CM.WriteLine($"{tag.Tag} ({tag.Digest})");
                }
                return 0;
            }

            private sealed class RepoOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--repo";
                public static string Description => "Repository of the image to tag";
            }
        }
    }
}
