using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SecurityCommand
    {
        private sealed partial class TasksCommand
        {
            private sealed class CreateCommand : IConsoleCommand
            {
                public static string Name => "create";
                public static string Description => "Creates a ProGet security task";
                public static string Examples => """
                      $> pgutil security tasks create --name="Basic Feed Access" --attributes=Feeds_ViewFeed
                      $> pgutil security tasks create --name="Developer Access" --attributes=Feeds_ViewFeed,Feeds_DownloadPackage,Feeds_PullPackage

                    For more information, see: https://docs.inedo.com/docs/proget/api/security/tasks/create
                    """;

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<NameOption>()
                        .WithOption<DescriptionOption>()
                        .WithOption<AttributesOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();

                    var name = context.GetOption<NameOption>();
                    var description = context.GetOption<DescriptionOption>();
                    var attributes = context.GetOption<AttributesOption>()
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    CM.WriteLine("Creating task ", new TextSpan(name, ConsoleColor.White), " with attributes ", string.Join(", ", attributes), "...");

                    if (context.HasFlag<ForceFlag>() && await client.ListSecurityTasksAsync(cancellationToken).AnyAsync(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase), cancellationToken))
                    {
                        await client.UpdateSecurityTaskAsync(
                            new SecurityTask
                            {
                                Name = name,
                                Description = description,
                                Attributes = attributes
                            },
                            cancellationToken
                        );
                    }
                    else
                    {
                        await client.CreateSecurityTaskAsync(
                            new SecurityTask
                            {
                                Name = name,
                                Description = description,
                                Attributes = attributes
                            },
                            cancellationToken
                        );
                    }

                    CM.WriteLine("Task created.");
                    return 0;
                }

                private sealed class ForceFlag : IConsoleFlagOption
                {
                    public static string Name => "--force";
                    public static string Description => "Overwrites an existing task with the same name if necessary";
                }
            }
        }
    }
}
