using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SecurityCommand
    {
        private sealed partial class GroupsCommand
        {
            private sealed partial class MembersCommand
            {
                private sealed class RemoveCommand : IConsoleCommand
                {
                    public static string Name => "remove";
                    public static string Description => "Removes a user from a group";
                    public static string Examples => """
                          $> pgutil security groups members remove --name=Developers --member="John Smith"
                        
                        For more information, see: https://docs.inedo.com/docs/proget/api/security/groups/remove-member
                        """;

                    public static void Configure(ICommandBuilder builder)
                    {
                        builder.WithOption<NameOption>()
                            .WithOption<MemberOption>();
                    }

                    public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                    {
                        var client = context.GetProGetClient();

                        var groupName = context.GetOption<NameOption>();
                        var member = context.GetOption<MemberOption>();

                        CM.WriteLine($"Removing user {member} from group {groupName}...");

                        var group = await client.ListUserGroups(cancellationToken)
                            .FirstOrDefaultAsync(g => g.Name.Equals(groupName, StringComparison.OrdinalIgnoreCase));

                        if (group is null)
                        {
                            CM.WriteError<NameOption>($"Group {groupName} not found.");
                            return -1;
                        }

                        var currentMembers = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        if (group.Users is not null)
                            currentMembers.UnionWith(group.Users);

                        if (!currentMembers.Remove(member))
                        {
                            CM.WriteLine($"User {member} is already not a member of group {groupName}.");
                            return 0;
                        }

                        await client.UpdateUserGroupAsync(
                            new SecurityGroup
                            {
                                Name = group.Name,
                                Users = [.. currentMembers]
                            },
                            cancellationToken
                        );

                        CM.WriteLine("User removed.");
                        return 0;
                    }
                }
            }
        }
    }
}
