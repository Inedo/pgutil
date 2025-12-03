using ConsoleMan;

namespace PgUtil;

internal partial class Program
{
    private sealed partial class SecurityCommand
    {
        private sealed partial class UsersCommand : IConsoleCommandContainer
        {
            public static string Name => "users";
            public static string Description => "Manages users in ProGet";

            public static void Configure(ICommandBuilder builder)
            {
                builder.WithCommand<CreateCommand>()
                    .WithCommand<EditCommand>()
                    .WithCommand<DeleteCommand>()
                    .WithCommand<ShowCommand>()
                    .WithCommand<ListCommand>();
            }

            private sealed class UserNameOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--username";
                public static string Description => "The username of the user";
            }

            private sealed class DisplayNameOption : IConsoleOption
            {
                public static bool Required => true;
                public static string Name => "--displayname";
                public static string Description => "The friendly display name of the user";
            }

            private sealed class EmailOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--email";
                public static string Description => "The user's email address";
            }

            private sealed class PasswordOption : IConsoleOption
            {
                public static bool Required => false;
                public static string Name => "--password";
                public static string Description => "Password to assign to the user account";
            }
        }
    }
}
