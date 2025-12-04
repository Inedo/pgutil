using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SecurityCommand
    {
        private sealed partial class TasksCommand
        {
            private sealed class DeleteCommand : IConsoleCommand
            {
                public static string Name => "delete";
                public static string Description => "Deletes a security task from ProGet";
                public static string Examples => """
                      $> pgutil security tasks delete --name="Basic Feed Access"

                    For more information, see: https://docs.inedo.com/docs/proget/api/security/tasks/delete
                    """;

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<NameOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    var name = context.GetOption<NameOption>();
                    CM.WriteLine("Deleting task ", new TextSpan(name, ConsoleColor.White), "...");
                    try
                    {
                        await client.DeleteSecurityTaskAsync(name, cancellationToken);
                        CM.WriteLine($"Task {name} deleted.");
                    }
                    catch (ProGetApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        CM.WriteLine($"Task {name} was not found; nothing to do.");
                    }

                    return 0;
                }
            }
        }
    }
}
