using ConsoleMan;

namespace PgUtil;

partial class Program
{
    partial class ContainersCommand
    {
        private sealed partial class ImagesCommand
        {
            private sealed class DeleteCommand : IConsoleCommand
            {
                public static string Name => "delete";
                public static string Description => "Deletes a container image from a ProGet feed";
                public static string Examples => """
                      $> pgutil containers images delete --feed=internal-docker --repo=proget --digest=sha256:072845d6e9a282e00aa465448698bf134c85655ebf2004527dcba19c657ccd47
                    """;

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<RepoOption>()
                        .WithOption<DigestOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    var feed = context.GetFeedName();
                    var repo = context.GetOption<RepoOption>();
                    var digest = context.GetOption<DigestOption>();

                    CM.WriteLine("Deleting image ", new TextSpan(digest, ConsoleColor.Blue), " from repository ", new TextSpan(repo, ConsoleColor.Blue), "...");
                    await client.DeleteContainerImageAsync(feed, repo, digest, cancellationToken);
                    CM.WriteLine("Image deleted.");
                    return 0;
                }

                private sealed class RepoOption : IConsoleOption
                {
                    public static bool Required => true;
                    public static string Name => "--repo";
                    public static string Description => "Repository of the image";
                }

                private sealed class DigestOption : IConsoleOption
                {
                    public static bool Required => true;
                    public static string Name => "--digest";
                    public static string Description => "Full digest of image to delete";
                }
            }
        }
    }
}
