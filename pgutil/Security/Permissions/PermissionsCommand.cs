using ConsoleMan;
using Inedo.ProGet;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SecurityCommand
    {
        private sealed partial class PermissionsCommand : IConsoleCommandContainer
        {
            public static string Name => "permissions";
            public static string Description => "Manage security permissions on ProGet";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithCommand<ListCommand>()
                    .WithCommand<AddCommand>()
                    .WithCommand<DeleteCommand>();
            }

            private static void FormatPermission(SecurityPermission p)
            {
                if (p.Deny)
                    CM.Write(ConsoleColor.Red, $"deny {p.Task}");
                else
                    CM.Write(ConsoleColor.Green, $"allow {p.Task}");

                Console.Write(' ');
                if (p.Feed is not null)
                    CM.Write("for feed ", new TextSpan(p.Feed, ConsoleColor.Blue));
                else if (p.FeedGroup is not null)
                    CM.Write("for feed group", new TextSpan(p.FeedGroup, ConsoleColor.Blue));
                else
                    CM.Write(ConsoleColor.Blue, "globally");

                CM.Write(" for ");
                if (p.User is not null)
                    CM.Write("user ", new TextSpan(p.User, ConsoleColor.DarkCyan));
                else if (p.Group is not null)
                    CM.Write("group ", new TextSpan(p.Group, ConsoleColor.DarkCyan));
                else
                    Console.Write("unknown");
            }
        }
    }
}
