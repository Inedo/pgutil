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
                public static string Examples => """
                      $> pgutil security groups create --name=Developers
                    
                    For more information, see: https://docs.inedo.com/docs/proget/api/security/groups/create
                    """;

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
