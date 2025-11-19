using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SecurityCommand
    {
        private sealed partial class TasksCommand
        {
            private sealed class ListCommand : IConsoleCommand
            {
                public static string Name => "list";
                public static string Description => "Lists security tasks in ProGet";

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<NameOption>(false);
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    var name = context.GetOptionOrDefault<NameOption>();

                    bool any = false;

                    await foreach (var task in client.ListSecurityTasksAsync(cancellationToken))
                    {
                        if (string.IsNullOrEmpty(name) || name.Equals(task.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            any = true;
                            CM.WriteLine(new TextSpan("Task: ", ConsoleColor.White), name);
                            CM.Write(new TextSpan("Description: ", ConsoleColor.White));
                            if (!string.IsNullOrWhiteSpace(task.Description))
                                CM.WriteLine(task.Description);
                            else
                                CM.WriteLine(ConsoleColor.DarkGray, "none");

                            CM.WriteLine(ConsoleColor.White, "Attributes: ");
                            foreach (var attr in task.Attributes)
                                CM.WriteLine($" * {attr}");

                            CM.WriteLine();
                        }
                    }

                    if (!any)
                    {
                        CM.WriteLine("No tasks found.");
                        CM.WriteLine();
                    }

                    return 0;
                }
            }
        }
    }
}
