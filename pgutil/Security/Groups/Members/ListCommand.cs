using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SecurityCommand
    {
        private sealed partial class GroupsCommand
        {
            private sealed partial class MembersCommand
            {
                private sealed class ListCommand : IConsoleCommand
                {
                    public static string Name => "list";
                    public static string Description => "List members of a group";
                    public static string Examples => """
                          $> pgutil security groups members list --name=Developers
                        
                        For more information, see: https://docs.inedo.com/docs/proget/api/security/groups/list-members
                        """;

                    public static void Configure(ICommandBuilder builder)
                    {
                        builder.WithOption<NameOption>();
                    }

                    public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                    {
                        var client = context.GetProGetClient();

                        var groupName = context.GetOption<NameOption>();
                        var group = await client.ListUserGroups(cancellationToken)
                            .FirstOrDefaultAsync(g => g.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase));

                        if (group is null)
                        {
                            CM.WriteError<NameOption>($"Group {groupName} not found.");
                            return -1;
                        }

                        if (group.Users?.Length > 0)
                        {
                            foreach (var user in group.Users)
                                CM.WriteLine(user);
                        }
                        else
                        {
                            CM.WriteLine("No members in group.");
                        }

                        return 0;
                    }
                }
            }
        }
    }
}
