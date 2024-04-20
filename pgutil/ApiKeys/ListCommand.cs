using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    internal sealed partial class ApiKeysCommand
    {
        private sealed class ListCommand : IConsoleCommand
        {
            public static string Name => "list";
            public static string Description => "List API keys";

            public static void Configure(ICommandBuilder builder)
            {
            }

            public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
            {
                var client = context.GetProGetClient();

                bool any = false;

                await foreach (var key in client.ListApiKeysAsync(cancellationToken))
                {
                    any = true;
                    CM.WriteLine($"{key.Id} {key.DisplayName}");
                    CM.WriteLine($"  {string.Join(", ", key.Apis ?? [])}");
                    CM.WriteLine();
                }

                if (!any)
                    CM.WriteLine("No API keys are defined.");

                return 0;
            }
        }
    }
}
