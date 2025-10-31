using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SecurityCommand
    {
        private sealed partial class GroupsCommand
        {
            private sealed class CreateCommand : IConsoleCommand
            {
                public static string Name => "create";
                public static string Description => "Creates a new user group";

                public static void Configure(ICommandBuilder builder)
                {
                    builder.WithOption<NameOption>();
                }

                public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                {
                    var client = context.GetProGetClient();
                    var name = context.GetOption<NameOption>();

                    CM.WriteLine("Creating group ", new TextSpan(name, ConsoleColor.White), "...");
                    await client.CreateUserGroupAsync(new SecurityGroup { Name = name }, cancellationToken);

                    CM.WriteLine("Group created.");
                    return 0;
                }
            }
        }
    }
}
