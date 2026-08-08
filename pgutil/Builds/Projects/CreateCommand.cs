using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class BuildsCommand
    {
        private sealed partial class ProjectsCommand
        {
            private sealed class CreateCommand : IConsoleCommand
            {
                public static string Name => "create";
                public static string Description => "Creates or updates project information";
                public static string Examples => """
                    >$ pgutil builds projects create --project=newProject
                    
                    >$ pgutil builds projects create --project=newApplication --type=Application
                    
                    >$ pgutil builds projects create --project=testApplication --type=Application --url=https://proget.corp.local

                    For more information, see: https://docs.inedo.com/docs/proget/api/sca/projects/create
                    """;
                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<ProjectOption>()
                        .WithOption<ProjectTypeOption>()
                        .WithOption<ProjectUrlOption>()
                        .WithOption<DescriptionOption>()
                        .WithOption<GroupOption>()
                        .WithOption<ProjectFeedsOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();

                    var info = new ProjectInfo
                    {
                        Name = context.GetOption<ProjectOption>(),
                        Type = context.GetOptionOrDefault<ProjectTypeOption>(),
                        Url = context.GetOptionOrDefault<ProjectUrlOption>(),
                        Description = context.GetOptionOrDefault<DescriptionOption>(),
                        Group = context.GetOptionOrDefault<GroupOption>(),
                        Feeds = context.GetOptionOrDefault<ProjectFeedsOption>()?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    };

                    var p = await client.CreateOrUpdateProjectAsync(info, cancellationToken).ConfigureAwait(false);
                    Console.WriteLine($"Project {p.Name} created/updated.");

                    return 0;
                }

                private sealed class ProjectOption : IConsoleOption
                {
                    public static bool Required => true;
                    public static string Name => "--project";
                    public static string Description => "Name of the project";
                }

                private sealed class ProjectTypeOption : IConsoleOption
                {
                    public static bool Required => false;
                    public static string Name => "--type";
                    public static string Description => "Type of the project";
                }

                private sealed class ProjectUrlOption : IConsoleOption
                {
                    public static bool Required => false;
                    public static string Name => "--url";
                    public static string Description => "URL of the project";
                }

                private sealed class DescriptionOption : IConsoleOption
                {
                    public static bool Required => false;
                    public static string Name => "--description";
                    public static string Description => "Description of the project";
                }

                private sealed class GroupOption : IConsoleOption
                {
                    public static bool Required => false;
                    public static string Name => "--group";
                    public static string Description => "Project group membership";
                }

                private sealed class ProjectFeedsOption : IConsoleOption
                {
                    public static bool Required => false;
                    public static string Name => "--project-feeds";
                    public static string Description => "Comma-separated list of feeds associated with the project";
                }
            }
        }
    }
}
