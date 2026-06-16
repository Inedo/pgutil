using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

partial class Program
{
    partial class ContainersCommand
    {
        private sealed partial class TagsCommand
        {
            private sealed class AddCommand : IConsoleCommand
            {
                public static string Name => "add";
                public static string Description => "Adds a tag to an image in ProGet";
                public static string Examples => """
                      $> pgutil containers tags add --feed=internal-docker --name=new_tag --repo=nginx --target=1.82
                      $> pgutil containers tags add --feed=internal-docker --name=pg_version --repo=proget --target=sha256:072845d6e9a282e00aa465448698bf134c85655ebf2004527dcba19c657ccd47
                    """;

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<NameOption>()
                        .WithOption<RepoOption>()
                        .WithOption<TargetOption>()
                        .WithOption<ForceFlag>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    var feed = context.GetFeedName();
                    var repo = context.GetOption<RepoOption>();
                    var target = context.GetOption<TargetOption>();
                    var name = context.GetOption<NameOption>();
                    bool force = context.HasFlag<ForceFlag>();

                    CM.WriteLine("Adding tag ", new TextSpan(name, ConsoleColor.Blue), " to ", new TextSpan($"{repo}:{target}", ConsoleColor.White), "...");
                    var status = await client.AddContainerImageTagAsync(feed, repo, name, target, force, cancellationToken);
                    CM.WriteLine(
                        status switch
                        {
                            AddTagResult.Exists => "Desired tag already exists.",
                            AddTagResult.Created => "Tag created.",
                            AddTagResult.Updated => "Tag updated.",
                            _ => status.ToString()
                        }
                    );
                    return 0;
                }

                private sealed class RepoOption : IConsoleOption
                {
                    public static bool Required => true;
                    public static string Name => "--repo";
                    public static string Description => "Repository of the image to tag";
                }

                private sealed class TargetOption : IConsoleOption
                {
                    public static bool Required => true;
                    public static string Name => "--target";
                    public static string Description => "Tag or digest that is the target of the new tag";
                }

                private sealed class NameOption : IConsoleOption
                {
                    public static bool Required => true;
                    public static string Name => "--name";
                    public static string Description => "Name of the new tag to create";
                }

                private sealed class ForceFlag : IConsoleFlagOption
                {
                    public static string Name => "--force";
                    public static string Description => "Forces creation of the tag even if it already exists and refers to a different target";
                }
            }
        }
    }
}
