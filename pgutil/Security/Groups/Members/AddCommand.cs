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
                private sealed class AddCommand : IConsoleCommand
                {
                    public static string Name => "add";
                    public static string Description => "Adds a user to a group";

                    public static void Configure(ICommandBuilder builder)
                    {
                        builder.WithOption<MemberOption>();
                    }

                    public static async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
                    {
                        var client = context.GetProGetClient();

                        var groupName = context.GetOption<NameOption>();
                        var member = context.GetOption<MemberOption>();

                        CM.WriteLine($"Adding user {member} to group {groupName}...");

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

                        if (!currentMembers.Add(member))
                        {
                            CM.WriteLine($"User {member} is already a member of group {groupName}.");
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

                        CM.WriteLine("User added.");
                        return 0;
                    }
                }
            }
        }
    }
}
