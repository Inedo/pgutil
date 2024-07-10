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
                public static string Description => "Create or update project information";

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<ProjectOption>()
                        .WithOption<ProjectTypeOption>()
                        .WithOption<ProjectUrlOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();

                    var info = new ProjectInfo
                    {
                        Name = context.GetOption<ProjectOption>(),
                        Type = context.GetOptionOrDefault<ProjectTypeOption>(),
                        Url = context.GetOptionOrDefault<ProjectUrlOption>()
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
            }
        }
    }
}
