using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SecurityCommand
    {
        private sealed partial class PermissionsCommand
        {
            private sealed class RemoveCommand : IConsoleCommand
            {
                public static string Name => "remove";
                public static string Description => "Removes a permission";

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<IdOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    int id = context.GetOption<IdOption, int>();
                    Console.WriteLine($"Removing permission id={id}...");
                    await client.DeletePermissionAsync(id, cancellationToken);
                    Console.WriteLine("Permission removed.");
                    return 0;
                }

                private sealed class IdOption : IConsoleOption
                {
                    public static bool Required => true;
                    public static string Name => "--id";
                    public static string Description => "Integer ID of the permission to remove";
                }
            }
        }
    }
}
